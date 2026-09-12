import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CategoryOrderBy } from '@app/core/models/enums/category-order-by';
import { OrderDirection } from '@app/core/models/enums/order-direction';
import { CategoryDetail, CategoryQuery } from '@app/core/models/interfaces/category';
import { ApiCategoriesService } from '@app/core/services/api/categories';
import { ToastService } from '@app/core/services/common/toast';
import { describeError } from '@app/core/utils/api-error';

const PAGE_SIZE = 10;
const SEARCH_DEBOUNCE_MS = 300;

/**
 * Which column the typed term is matched against. The API ANDs `Name` and
 * `Description` together, so searching both at once would only return the
 * categories that match in both places - the reader picks one.
 */
export const SearchField = {
  Name: 'name',
  Description: 'description',
} as const;
export type SearchField = (typeof SearchField)[keyof typeof SearchField];

export interface SearchFieldOption {
  readonly label: string;
  readonly value: SearchField;
}

export const SEARCH_FIELDS: readonly SearchFieldOption[] = [
  { label: 'Name', value: SearchField.Name },
  { label: 'Description', value: SearchField.Description },
];

export interface SortOption {
  readonly label: string;
  readonly value: string;
  readonly orderBy: CategoryOrderBy;
  readonly direction: OrderDirection;
}

export const SORT_OPTIONS: readonly SortOption[] = [
  {
    label: 'Newest first',
    value: 'newest',
    orderBy: CategoryOrderBy.CreatedAt,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Oldest first',
    value: 'oldest',
    orderBy: CategoryOrderBy.CreatedAt,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Name: A to Z',
    value: 'name-asc',
    orderBy: CategoryOrderBy.Name,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Name: Z to A',
    value: 'name-desc',
    orderBy: CategoryOrderBy.Name,
    direction: OrderDirection.Desc,
  },
];

/**
 * The management page's state: the filters the admin has chosen, the page they are
 * on, and the delete they have in flight. Page-scoped, so a fresh visit starts
 * unfiltered.
 */
@Injectable()
export class CategoriesStore {
  private readonly api = inject(ApiCategoriesService);
  private readonly toasts = inject(ToastService);

  readonly searchFields = SEARCH_FIELDS;
  readonly sortOptions = SORT_OPTIONS;
  readonly pageSize = PAGE_SIZE;

  readonly searchInput = signal('');

  /** Debounced so typing does not fire a request per keystroke. */
  private readonly search = toSignal(
    toObservable(this.searchInput).pipe(debounceTime(SEARCH_DEBOUNCE_MS), distinctUntilChanged()),
    { initialValue: '' },
  );

  readonly searchField = signal<SearchField>(SearchField.Name);
  readonly sortKey = signal(SORT_OPTIONS[0].value);

  /** Writable for the pager, but any filter change sends the admin back to page 1. */
  readonly page = linkedSignal<string, number>({
    source: () => `${this.search()}|${this.searchField()}|${this.sortKey()}`,
    computation: () => 1,
  });

  private readonly query = computed<CategoryQuery>(() => {
    const sort = SORT_OPTIONS.find((option) => option.value === this.sortKey()) ?? SORT_OPTIONS[0];
    const term = this.search();
    const field = this.searchField();

    return {
      page: this.page(),
      pageSize: PAGE_SIZE,
      name: field === SearchField.Name ? term : '',
      description: field === SearchField.Description ? term : '',
      orderBy: sort.orderBy,
      orderByDirection: sort.direction,
    };
  });

  private readonly categories = this.api.search(this.query);

  private readonly deletingIdState = signal<string | null>(null);

  /** The category whose delete is in flight, so its row can show the progress. */
  readonly deletingId = this.deletingIdState.asReadonly();

  readonly isLoading = computed(() => this.categories.isLoading());

  // Reading value() on a failed resource rethrows, so gate every read.
  readonly items = computed<readonly CategoryDetail[]>(() =>
    this.categories.hasValue() ? (this.categories.value()?.data ?? []) : [],
  );
  readonly totalCount = computed(() =>
    this.categories.hasValue() ? (this.categories.value()?.totalCount ?? 0) : 0,
  );
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)));
  readonly errorMessage = computed(() => describeError(this.categories.error()));

  readonly hasFilters = computed(() => this.searchInput().trim().length > 0);

  setSearch(term: string): void {
    this.searchInput.set(term);
  }

  setSearchField(field: SearchField): void {
    this.searchField.set(field);
  }

  setSort(key: string): void {
    this.sortKey.set(key);
  }

  clearFilters(): void {
    this.searchInput.set('');
    this.searchField.set(SearchField.Name);
  }

  goToPage(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  reload(): void {
    this.categories.reload();
  }

  /**
   * Soft-deletes the category and refreshes the listing. A failure is reported by
   * `errorInterceptor` and leaves the row where it was.
   */
  remove(category: CategoryDetail): void {
    if (this.deletingIdState() !== null) {
      return;
    }

    this.deletingIdState.set(category.id);

    this.api.remove(category.id).subscribe({
      next: () => {
        this.deletingIdState.set(null);
        this.toasts.success('Category deleted successfully.');
        this.afterRemoval();
      },
      error: () => this.deletingIdState.set(null),
    });
  }

  /**
   * Emptying the last page would otherwise strand the admin on a page that no
   * longer exists, so stepping back refetches; every other page just reloads.
   */
  private afterRemoval(): void {
    if (this.items().length === 1 && this.page() > 1) {
      this.page.set(this.page() - 1);
      return;
    }

    this.categories.reload();
  }
}
