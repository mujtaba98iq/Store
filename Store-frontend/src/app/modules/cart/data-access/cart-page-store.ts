import { Injectable, computed, inject, signal } from '@angular/core';
import { CartItem } from '@app/core/models/interfaces/cart';
import { CartStore } from '@app/core/services/common/cart-store';
import { ApiCartVariantsLookupService } from '../api/product-variants';
import { ProductVariant } from '../models/product-variant.model';

/**
 * One row of the cart page: the line as the API holds it, plus what it takes to
 * read that line - a variant id is not something anyone recognises.
 */
export interface CartLine {
  readonly item: CartItem;
  /** What the row is called: the product's name, or its SKU when that is all there is. */
  readonly label: string;
  /** Null while the lookup is still out, or if the variant behind the line is gone. */
  readonly sku: string | null;
  /** Where the row links back to. Null when the variant can no longer be resolved. */
  readonly productId: string | null;
}

/**
 * The cart page's own state.
 *
 * The cart itself is not here - it lives in `CartStore`, because the header badge
 * counts the same lines. What this adds is the labelling the page needs and the
 * removal the page asks to confirm.
 */
@Injectable()
export class CartPageStore {
  private readonly cart = inject(CartStore);
  private readonly variantsApi = inject(ApiCartVariantsLookupService);

  private readonly variantsResource = this.variantsApi.list();

  // Reading value() on a failed resource rethrows, so gate every read.
  private readonly variantsById = computed(() => {
    const variants: readonly ProductVariant[] = this.variantsResource.hasValue()
      ? (this.variantsResource.value()?.data ?? [])
      : [];

    return new Map(variants.map((variant) => [variant.id, variant]));
  });

  readonly isLoading = computed(() => this.cart.isLoading());
  readonly errorMessage = this.cart.errorMessage;
  readonly isEmpty = this.cart.isEmpty;
  readonly isLoaded = this.cart.isLoaded;
  readonly totalAmount = this.cart.totalAmount;
  readonly unitCount = this.cart.unitCount;
  readonly lineCount = this.cart.lineCount;
  readonly pendingItemId = this.cart.pendingItemId;
  readonly isClearing = this.cart.isClearing;
  readonly isBusy = this.cart.isBusy;

  /**
   * The lines, labelled. A line whose variant the lookup has not answered for yet -
   * or cannot answer for at all - keeps its place and its price rather than being
   * hidden: it is still in the cart and still being charged for.
   */
  readonly lines = computed<readonly CartLine[]>(() =>
    this.cart.items().map((item) => {
      const variant = this.variantsById().get(item.productVariantId);

      return {
        item,
        label: variant?.productName ?? variant?.sku ?? 'This item',
        sku: variant?.sku ?? null,
        productId: variant?.productId ?? null,
      };
    }),
  );

  /** The line the shopper has asked to remove, held until they confirm. */
  readonly pendingRemoval = signal<CartLine | null>(null);
  readonly isClearConfirmOpen = signal(false);

  setQuantity(line: CartLine, quantity: number): void {
    // Stepping below one is a removal, and the API refuses a quantity of zero, so
    // the shopper is asked to confirm rather than being sent a request that fails.
    if (quantity < 1) {
      this.askToRemove(line);
      return;
    }

    if (quantity !== line.item.quantity) {
      this.cart.updateQuantity(line.item.id, quantity);
    }
  }

  askToRemove(line: CartLine): void {
    this.pendingRemoval.set(line);
  }

  cancelRemoval(): void {
    this.pendingRemoval.set(null);
  }

  confirmRemoval(): void {
    const line = this.pendingRemoval();
    this.pendingRemoval.set(null);

    if (line) {
      this.cart.remove(line.item.id);
    }
  }

  askToClear(): void {
    this.isClearConfirmOpen.set(true);
  }

  cancelClear(): void {
    this.isClearConfirmOpen.set(false);
  }

  confirmClear(): void {
    this.isClearConfirmOpen.set(false);
    this.cart.clear();
  }

  reload(): void {
    this.cart.load();
    this.variantsResource.reload();
  }
}
