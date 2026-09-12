import { Injectable, computed, inject, linkedSignal, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { OrderDirection } from '@app/core/models/enums/order-direction';
import { ToastService } from '@app/core/services/common/toast';
import { describeError } from '@app/core/utils/api-error';
import { ApiInventoriesService } from '../api/inventories';
import { ApiProductVariantsService } from '../api/product-variants';
import { Inventory, InventoryOrderBy, InventoryQuery } from '../models/inventory.model';
import { ProductVariant } from '../models/product-variant.model';

const PAGE_SIZE = 10;
const SEARCH_DEBOUNCE_MS = 300;

export interface AvailabilityOption {
  readonly label: string;
  readonly value: string;
  /** `null` is the "everything" pill: the filter is left off the request. */
  readonly isAvailable: boolean | null;
}

export const AVAILABILITY_FILTERS: readonly AvailabilityOption[] = [
  { label: 'All stock', value: 'all', isAvailable: null },
  { label: 'In stock', value: 'in', isAvailable: true },
  { label: 'Sold out', value: 'out', isAvailable: false },
];

export interface SortOption {
  readonly label: string;
  readonly value: string;
  readonly orderBy: InventoryOrderBy;
  readonly direction: OrderDirection;
}

export const SORT_OPTIONS: readonly SortOption[] = [
  {
    label: 'Newest first',
    value: 'newest',
    orderBy: InventoryOrderBy.CreatedAt,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Oldest first',
    value: 'oldest',
    orderBy: InventoryOrderBy.CreatedAt,
    direction: OrderDirection.Asc,
  },
  {
    label: 'SKU: A to Z',
    value: 'sku-asc',
    orderBy: InventoryOrderBy.Sku,
    direction: OrderDirection.Asc,
  },
  {
    label: 'SKU: Z to A',
    value: 'sku-desc',
    orderBy: InventoryOrderBy.Sku,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Least available first',
    value: 'available-asc',
    orderBy: InventoryOrderBy.AvailableQuantity,
    direction: OrderDirection.Asc,
  },
  {
    label: 'Most available first',
    value: 'available-desc',
    orderBy: InventoryOrderBy.AvailableQuantity,
    direction: OrderDirection.Desc,
  },
  {
    label: 'Most on hand first',
    value: 'quantity-desc',
    orderBy: InventoryOrderBy.Quantity,
    direction: OrderDirection.Desc,
  },
];

/** What the admin has typed into the three free-text boxes of the toolbar. */
interface TypedFilters {
  readonly sku: string;
  readonly minQuantity: number | null;
  readonly maxQuantity: number | null;
}

const NO_TYPED_FILTERS: TypedFilters = { sku: '', minQuantity: null, maxQuantity: null };

function sameTypedFilters(left: TypedFilters, right: TypedFilters): boolean {
  return (
    left.sku === right.sku &&
    left.minQuantity === right.minQuantity &&
    left.maxQuantity === right.maxQuantity
  );
}

/**
 * A blank box means "no bound", not zero - and a half-typed "1e" is not a bound
 * either, so anything that is not a whole count at or above zero is left off the
 * request rather than sent for the API to refuse.
 */
function toBound(raw: string): number | null {
  const trimmed = raw.trim();
  if (!trimmed) {
    return null;
  }

  const value = Number(trimmed);
  return Number.isInteger(value) && value >= 0 ? value : null;
}

/**
 * The management page's state: the filters the admin has chosen, the page they are
 * on, and the delete they have in flight. Page-scoped, so a fresh visit starts
 * unfiltered.
 */
@Injectable()
export class InventoriesStore {
  private readonly api = inject(ApiInventoriesService);
  private readonly variantsApi = inject(ApiProductVariantsService);
  private readonly toasts = inject(ToastService);

  readonly availabilityFilters = AVAILABILITY_FILTERS;
  readonly sortOptions = SORT_OPTIONS;
  readonly pageSize = PAGE_SIZE;

  readonly searchInput = signal('');
  readonly minInput = signal('');
  readonly maxInput = signal('');

  private readonly typed = computed<TypedFilters>(() => ({
    sku: this.searchInput().trim(),
    minQuantity: toBound(this.minInput()),
    maxQuantity: toBound(this.maxInput()),
  }));

  /**
   * Debounced so typing does not fire a request per keystroke. All three boxes share
   * one countdown: filling in a floor and then a ceiling is one thought, and should
   * cost one request rather than two.
   */
  private readonly filters = toSignal(
    toObservable(this.typed).pipe(
      debounceTime(SEARCH_DEBOUNCE_MS),
      distinctUntilChanged(sameTypedFilters),
    ),
    { initialValue: NO_TYPED_FILTERS },
  );

  readonly availability = signal(AVAILABILITY_FILTERS[0].value);
  readonly sortKey = signal(SORT_OPTIONS[0].value);

  /** Writable for the pager, but any filter change sends the admin back to page 1. */
  readonly page = linkedSignal<string, number>({
    source: () => {
      const { sku, minQuantity, maxQuantity } = this.filters();
      return `${sku}|${minQuantity}|${maxQuantity}|${this.availability()}|${this.sortKey()}`;
    },
    computation: () => 1,
  });

  private readonly query = computed<InventoryQuery>(() => {
    const sort = SORT_OPTIONS.find((option) => option.value === this.sortKey()) ?? SORT_OPTIONS[0];
    const availability =
      AVAILABILITY_FILTERS.find((option) => option.value === this.availability()) ??
      AVAILABILITY_FILTERS[0];
    const { sku, minQuantity, maxQuantity } = this.filters();

    return {
      page: this.page(),
      pageSize: PAGE_SIZE,
      sku,
      isAvailable: availability.isAvailable,
      minQuantity,
      maxQuantity,
      orderBy: sort.orderBy,
      orderByDirection: sort.direction,
    };
  });

  private readonly inventories = this.api.search(this.query);
  private readonly variants = this.variantsApi.list();

  private readonly deletingIdState = signal<string | null>(null);

  /** The row whose delete is in flight, so it can show the progress. */
  readonly deletingId = this.deletingIdState.asReadonly();

  readonly isLoading = computed(() => this.inventories.isLoading());

  // Reading value() on a failed resource rethrows, so gate every read.
  readonly items = computed<readonly Inventory[]>(() =>
    this.inventories.hasValue() ? (this.inventories.value()?.data ?? []) : [],
  );
  readonly totalCount = computed(() =>
    this.inventories.hasValue() ? (this.inventories.value()?.totalCount ?? 0) : 0,
  );
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)));
  readonly errorMessage = computed(() => describeError(this.inventories.error()));

  /** The variants a new stock row can be opened against, for the form's picker. */
  readonly variantOptions = computed<readonly ProductVariant[]>(() =>
    this.variants.hasValue() ? (this.variants.value()?.data ?? []) : [],
  );
  readonly variantsLoading = computed(() => this.variants.isLoading());

  readonly hasFilters = computed(
    () =>
      this.searchInput().trim().length > 0 ||
      this.minInput().trim().length > 0 ||
      this.maxInput().trim().length > 0 ||
      this.availability() !== AVAILABILITY_FILTERS[0].value,
  );

  setSearch(term: string): void {
    this.searchInput.set(term);
  }

  setMinQuantity(value: string): void {
    this.minInput.set(value);
  }

  setMaxQuantity(value: string): void {
    this.maxInput.set(value);
  }

  setAvailability(value: string): void {
    this.availability.set(value);
  }

  setSort(key: string): void {
    this.sortKey.set(key);
  }

  clearFilters(): void {
    this.searchInput.set('');
    this.minInput.set('');
    this.maxInput.set('');
    this.availability.set(AVAILABILITY_FILTERS[0].value);
  }

  goToPage(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  reload(): void {
    this.inventories.reload();
  }

  /**
   * Refetches the listing, and the variant lookup with it: a row that was just
   * created takes its variant out of the picker, and one that was deleted frees
   * its variant to be stocked again.
   */
  reloadAll(): void {
    this.inventories.reload();
    this.variants.reload();
  }

  /**
   * Soft-deletes the stock row and refreshes the listing. A failure is reported by
   * `errorInterceptor` and leaves the row where it was.
   */
  remove(inventory: Inventory): void {
    if (this.deletingIdState() !== null) {
      return;
    }

    this.deletingIdState.set(inventory.id);

    this.api.remove(inventory.id).subscribe({
      next: () => {
        this.deletingIdState.set(null);
        this.toasts.success('Stock row deleted successfully.');
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
    // The deleted row freed its variant to be stocked again, so the picker is stale
    // whichever page the admin ends up on.
    this.variants.reload();

    if (this.items().length === 1 && this.page() > 1) {
      this.page.set(this.page() - 1);
      return;
    }

    this.inventories.reload();
  }
}
