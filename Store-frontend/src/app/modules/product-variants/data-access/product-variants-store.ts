import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { OrderDirection } from '@app/core/models/enums/order-direction';
import { ToastService } from '@app/core/services/common/toast';
import { describeError } from '@app/core/utils/api-error';
import { ApiProductsLookupService } from '../api/products';
import { ApiProductVariantsService } from '../api/product-variants';
import { ProductOption } from '../models/product.model';
import {
  ProductVariant,
  ProductVariantOrderBy,
  ProductVariantQuery,
} from '../models/product-variant.model';

const PAGE_SIZE = 10;
const SEARCH_DEBOUNCE_MS = 300;

export interface StatusOption {
  readonly label: string;
  readonly value: string;
  /** `null` is the "everything" pill: the filter is left off the request. */
  readonly isActive: boolean | null;
}

export const STATUS_FILTERS: readonly StatusOption[] = [
  { label: 'All variants', value: 'all', isActive: null },
  { label: 'Active', value: 'active', isActive: true },
  { label: 'Inactive', value: 'inactive', isActive: false },
];

export interface SortOption {
  readonly label: string;
  readonly value: string;
  readonly orderBy: ProductVariantOrderBy;
  readonly direction: OrderDirection;
}

export const SORT_OPTIONS: readonly SortOption[] = [
  {
    label: 'Newest first',
    value: 'newest',
    orderBy: ProductVariantOrderBy.CreatedAt,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Oldest first',
    value: 'oldest',
    orderBy: ProductVariantOrderBy.CreatedAt,
    direction: OrderDirection.Asc,
  },
  {
    label: 'SKU: A to Z',
    value: 'sku-asc',
    orderBy: ProductVariantOrderBy.Sku,
    direction: OrderDirection.Asc,
  },
  {
    label: 'SKU: Z to A',
    value: 'sku-desc',
    orderBy: ProductVariantOrderBy.Sku,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Price: low to high',
    value: 'price-asc',
    orderBy: ProductVariantOrderBy.Price,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Price: high to low',
    value: 'price-desc',
    orderBy: ProductVariantOrderBy.Price,
    direction: OrderDirection.Desc,
  },
];

/** What the admin has typed into the two free-text boxes of the toolbar. */
interface TypedFilters {
  readonly sku: string;
  readonly barcode: string;
}

const NO_TYPED_FILTERS: TypedFilters = { sku: '', barcode: '' };

function sameTypedFilters(left: TypedFilters, right: TypedFilters): boolean {
  return left.sku === right.sku && left.barcode === right.barcode;
}

/**
 * The management page's state: the filters the admin has chosen, the page they are
 * on, and the delete they have in flight. Page-scoped, so a fresh visit starts
 * unfiltered.
 */
@Injectable()
export class ProductVariantsStore {
  private readonly api = inject(ApiProductVariantsService);
  private readonly productsApi = inject(ApiProductsLookupService);
  private readonly toasts = inject(ToastService);

  readonly statusFilters = STATUS_FILTERS;
  readonly sortOptions = SORT_OPTIONS;
  readonly pageSize = PAGE_SIZE;

  readonly searchInput = signal('');
  readonly barcodeInput = signal('');

  private readonly typed = computed<TypedFilters>(() => ({
    sku: this.searchInput().trim(),
    barcode: this.barcodeInput().trim(),
  }));

  /**
   * Debounced so typing does not fire a request per keystroke. Both boxes share one
   * countdown: narrowing by SKU and then by barcode is one thought, and should cost
   * one request rather than two.
   */
  private readonly filters = toSignal(
    toObservable(this.typed).pipe(
      debounceTime(SEARCH_DEBOUNCE_MS),
      distinctUntilChanged(sameTypedFilters),
    ),
    { initialValue: NO_TYPED_FILTERS },
  );

  /** The chosen product, or empty for every product. */
  readonly productId = signal('');
  readonly status = signal(STATUS_FILTERS[0].value);
  readonly sortKey = signal(SORT_OPTIONS[0].value);

  /** Writable for the pager, but any filter change sends the admin back to page 1. */
  readonly page = linkedSignal<string, number>({
    source: () => {
      const { sku, barcode } = this.filters();
      return [sku, barcode, this.productId(), this.status(), this.sortKey()].join('|');
    },
    computation: () => 1,
  });

  private readonly query = computed<ProductVariantQuery>(() => {
    const sort = SORT_OPTIONS.find((option) => option.value === this.sortKey()) ?? SORT_OPTIONS[0];
    const status =
      STATUS_FILTERS.find((option) => option.value === this.status()) ?? STATUS_FILTERS[0];
    const { sku, barcode } = this.filters();

    return {
      page: this.page(),
      pageSize: PAGE_SIZE,
      sku,
      barcode,
      productId: this.productId() || null,
      isActive: status.isActive,
      orderBy: sort.orderBy,
      orderByDirection: sort.direction,
    };
  });

  private readonly variants = this.api.search(this.query);
  private readonly products = this.productsApi.list();

  private readonly deletingIdState = signal<string | null>(null);

  /** The row whose delete is in flight, so it can show the progress. */
  readonly deletingId = this.deletingIdState.asReadonly();

  readonly isLoading = computed(() => this.variants.isLoading());

  // Reading value() on a failed resource rethrows, so gate every read.
  readonly items = computed<readonly ProductVariant[]>(() =>
    this.variants.hasValue() ? (this.variants.value()?.data ?? []) : [],
  );
  readonly totalCount = computed(() =>
    this.variants.hasValue() ? (this.variants.value()?.totalCount ?? 0) : 0,
  );
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)));
  readonly errorMessage = computed(() => describeError(this.variants.error()));

  /** The products a variant can be opened against, for the filter and the picker. */
  readonly productOptions = computed<readonly ProductOption[]>(() =>
    this.products.hasValue() ? (this.products.value()?.data ?? []) : [],
  );
  readonly productsLoading = computed(() => this.products.isLoading());

  readonly hasFilters = computed(
    () =>
      this.searchInput().trim().length > 0 ||
      this.barcodeInput().trim().length > 0 ||
      this.productId().length > 0 ||
      this.status() !== STATUS_FILTERS[0].value,
  );

  setSearch(term: string): void {
    this.searchInput.set(term);
  }

  setBarcode(term: string): void {
    this.barcodeInput.set(term);
  }

  setProduct(productId: string): void {
    this.productId.set(productId);
  }

  setStatus(value: string): void {
    this.status.set(value);
  }

  setSort(key: string): void {
    this.sortKey.set(key);
  }

  clearFilters(): void {
    this.searchInput.set('');
    this.barcodeInput.set('');
    this.productId.set('');
    this.status.set(STATUS_FILTERS[0].value);
  }

  goToPage(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  reload(): void {
    this.variants.reload();
  }

  /**
   * Soft-deletes the variant and refreshes the listing. A failure is reported by
   * `errorInterceptor` and leaves the row where it was - a variant that still has
   * stock open against it is refused, and the admin is told to clear that first.
   */
  remove(variant: ProductVariant): void {
    if (this.deletingIdState() !== null) {
      return;
    }

    this.deletingIdState.set(variant.id);

    this.api.remove(variant.id).subscribe({
      next: () => {
        this.deletingIdState.set(null);
        this.toasts.success('Product variant deleted successfully.');
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

    this.variants.reload();
  }
}
