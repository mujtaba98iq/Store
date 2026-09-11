import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { OrderDirection } from '@app/core/models/enums/order-direction';
import { Category } from '@app/core/models/interfaces/category';
import { ApiCategoriesService } from '@app/core/services/api/categories';
import { describeError } from '@app/core/utils/api-error';
import { ApiProductsService } from '../api/products';
import { Product, ProductOrderBy, ProductQuery } from '../models/product.model';

const PAGE_SIZE = 12;
const SEARCH_DEBOUNCE_MS = 300;

export interface CategoryFilter {
  readonly label: string;
  /** `null` is the "All" filter. */
  readonly value: string | null;
}

export interface SortOption {
  readonly label: string;
  readonly value: string;
  readonly orderBy: ProductOrderBy;
  readonly direction: OrderDirection;
}

export const SORT_OPTIONS: readonly SortOption[] = [
  {
    label: 'Newest first',
    value: 'newest',
    orderBy: ProductOrderBy.CreatedAt,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Oldest first',
    value: 'oldest',
    orderBy: ProductOrderBy.CreatedAt,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Price: low to high',
    value: 'price-asc',
    orderBy: ProductOrderBy.Price,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Price: high to low',
    value: 'price-desc',
    orderBy: ProductOrderBy.Price,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Name: A to Z',
    value: 'name-asc',
    orderBy: ProductOrderBy.Name,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Name: Z to A',
    value: 'name-desc',
    orderBy: ProductOrderBy.Name,
    direction: OrderDirection.Desc,
  },
];

/**
 * The catalogue page's state: the filters the reader has chosen and the page of
 * products they select. Page-scoped, so a fresh visit starts unfiltered.
 */
@Injectable()
export class ProductsStore {
  private readonly api = inject(ApiProductsService);
  private readonly categoriesApi = inject(ApiCategoriesService);

  readonly sortOptions = SORT_OPTIONS;
  readonly pageSize = PAGE_SIZE;

  readonly searchInput = signal('');

  /** Debounced so typing does not fire a request per keystroke. */
  private readonly search = toSignal(
    toObservable(this.searchInput).pipe(debounceTime(SEARCH_DEBOUNCE_MS), distinctUntilChanged()),
    { initialValue: '' },
  );

  readonly activeCategoryId = signal<string | null>(null);
  readonly sortKey = signal(SORT_OPTIONS[0].value);

  /** Writable for the pager, but any filter change sends the reader back to page 1. */
  readonly page = linkedSignal<string, number>({
    source: () => `${this.search()}|${this.activeCategoryId()}|${this.sortKey()}`,
    computation: () => 1,
  });

  private readonly query = computed<ProductQuery>(() => {
    const sort = SORT_OPTIONS.find((option) => option.value === this.sortKey()) ?? SORT_OPTIONS[0];
    return {
      page: this.page(),
      pageSize: PAGE_SIZE,
      name: this.search(),
      categoryId: this.activeCategoryId(),
      orderBy: sort.orderBy,
      orderByDirection: sort.direction,
    };
  });

  private readonly products = this.api.list(this.query);
  private readonly categories = this.categoriesApi.list();

  readonly isLoading = computed(() => this.products.isLoading());

  // Reading value() on a failed resource rethrows, so gate every read.
  readonly items = computed<readonly Product[]>(() =>
    this.products.hasValue() ? (this.products.value()?.data ?? []) : [],
  );
  readonly totalCount = computed(() =>
    this.products.hasValue() ? (this.products.value()?.totalCount ?? 0) : 0,
  );
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)));
  readonly errorMessage = computed(() => describeError(this.products.error()));

  readonly categoryList = computed<readonly Category[]>(() =>
    this.categories.hasValue() ? (this.categories.value()?.data ?? []) : [],
  );

  readonly filters = computed<readonly CategoryFilter[]>(() => [
    { label: 'All', value: null },
    ...this.categoryList().map((category) => ({ label: category.name, value: category.id })),
  ]);

  setSearch(term: string): void {
    this.searchInput.set(term);
  }

  selectCategory(categoryId: string | null): void {
    this.activeCategoryId.set(categoryId);
  }

  setSort(key: string): void {
    this.sortKey.set(key);
  }

  clearFilters(): void {
    this.searchInput.set('');
    this.activeCategoryId.set(null);
  }

  goToPage(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  reload(): void {
    this.products.reload();
  }
}
