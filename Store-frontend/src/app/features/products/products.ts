import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  linkedSignal,
  signal,
} from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { describeError } from '../../core/api/api-error';
import { AuthStore } from '../../core/auth/auth-store';
import { ProductCard } from '../../shared/components/product-card/product-card';
import { categoriesResource } from '../categories/categories-api';
import { Modal } from '../../shared/components/modal/modal';
import { ProductForm } from './components/product-form/product-form';
import {
  Category,
  OrderDirection,
  Product,
  ProductOrderBy,
  ProductQuery,
  productImageUrl,
} from './product';
import { productsResource } from './products-api';

const PAGE_SIZE = 12;
const SEARCH_DEBOUNCE_MS = 300;

interface CategoryFilter {
  readonly label: string;
  /** `null` is the "All" filter. */
  readonly value: string | null;
}

interface SortOption {
  readonly label: string;
  readonly value: string;
  readonly orderBy: ProductOrderBy;
  readonly direction: OrderDirection;
}

const SORT_OPTIONS: readonly SortOption[] = [
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

@Component({
  selector: 'app-products',
  imports: [Modal, ProductCard, ProductForm],
  templateUrl: './products.html',
  styleUrl: './products.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Products {
  private readonly auth = inject(AuthStore);

  protected readonly isAdmin = this.auth.isAdmin;
  protected readonly sortOptions = SORT_OPTIONS;

  protected readonly searchInput = signal('');

  /** Debounced so typing does not fire a request per keystroke. */
  private readonly search = toSignal(
    toObservable(this.searchInput).pipe(debounceTime(SEARCH_DEBOUNCE_MS), distinctUntilChanged()),
    { initialValue: '' },
  );

  protected readonly activeCategoryId = signal<string | null>(null);
  protected readonly sortKey = signal(SORT_OPTIONS[0].value);

  /** Writable for the pager, but any filter change sends the reader back to page 1. */
  protected readonly page = linkedSignal<string, number>({
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

  protected readonly products = productsResource(this.query);
  protected readonly categories = categoriesResource();

  // Reading value() on a failed resource rethrows, so gate every read.
  protected readonly items = computed<readonly Product[]>(() =>
    this.products.hasValue() ? (this.products.value()?.data ?? []) : [],
  );
  protected readonly totalCount = computed(() =>
    this.products.hasValue() ? (this.products.value()?.totalCount ?? 0) : 0,
  );
  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)),
  );
  protected readonly errorMessage = computed(() => describeError(this.products.error()));

  protected readonly categoryList = computed<readonly Category[]>(() =>
    this.categories.hasValue() ? (this.categories.value()?.data ?? []) : [],
  );

  protected readonly filters = computed<readonly CategoryFilter[]>(() => [
    { label: 'All', value: null },
    ...this.categoryList().map((category) => ({ label: category.name, value: category.id })),
  ]);

  /** Uploaded image first, then the legacy imagePath. */
  protected readonly imageFor = productImageUrl;

  protected readonly editing = signal<Product | null>(null);
  protected readonly isFormOpen = signal(false);
  protected readonly lastAdded = signal('');

  protected selectCategory(categoryId: string | null): void {
    this.activeCategoryId.set(categoryId);
  }

  protected onSearch(event: Event): void {
    this.searchInput.set((event.target as HTMLInputElement).value);
  }

  protected onSortChange(event: Event): void {
    this.sortKey.set((event.target as HTMLSelectElement).value);
  }

  protected clearFilters(): void {
    this.searchInput.set('');
    this.activeCategoryId.set(null);
  }

  protected goToPage(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  protected retry(): void {
    this.products.reload();
  }

  protected openCreate(): void {
    this.editing.set(null);
    this.isFormOpen.set(true);
  }

  protected openEdit(product: Product): void {
    this.editing.set(product);
    this.isFormOpen.set(true);
  }

  protected closeForm(): void {
    this.isFormOpen.set(false);
  }

  protected onSaved(): void {
    this.isFormOpen.set(false);
    this.products.reload();
  }

  // No cart service yet - announce the action so the button is not a dead end.
  protected addToBag(product: Product): void {
    this.lastAdded.set(product.name);
  }
}
