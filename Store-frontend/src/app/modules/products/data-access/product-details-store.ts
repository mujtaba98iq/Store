import { Injectable, computed, inject, linkedSignal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
import { AuthStore } from '@app/core/services/common/auth-store';
import { CartStore } from '@app/core/services/common/cart-store';
import { describeError } from '@app/core/utils/api-error';
import { ApiProductsService } from '../api/products';
import { Product } from '../models/product.model';
import { ProductVariant } from '../models/product-variant.model';

/**
 * Far more than anyone buys at once, and the only bound the shop applies - the API
 * checks that a quantity is positive and nothing else.
 */
const MAX_QUANTITY = 99;

/**
 * One product page: the product, the variants it can be bought in, and the choice
 * the shopper is part-way through making.
 *
 * Page-scoped. The cart the choice ends up in is not - that lives in `CartStore`,
 * because the header counts it too.
 */
@Injectable()
export class ProductDetailsStore {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiProductsService);
  private readonly auth = inject(AuthStore);
  private readonly cart = inject(CartStore);

  readonly maxQuantity = MAX_QUANTITY;

  /**
   * Read from the parameter map rather than a snapshot, so following a link from
   * one product to another reloads the page's data instead of stranding it on the
   * id it was first built with.
   */
  readonly productId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id') ?? '')), {
    initialValue: '',
  });

  readonly isSignedIn = this.auth.isSignedIn;

  private readonly productResource = this.api.getById(this.productId);

  // Reading value() on a failed resource rethrows, so gate every read.
  readonly product = computed<Product | null>(() =>
    this.productResource.hasValue() ? (this.productResource.value() ?? null) : null,
  );
  readonly isLoading = computed(() => this.productResource.isLoading());
  readonly errorMessage = computed(() => describeError(this.productResource.error()));

  /**
   * What can actually be bought. The API carries every live variant on the product,
   * inactive ones included, and refuses to cart an inactive one - so offering it
   * would only be a button that fails.
   *
   * Taking them from the product rather than from the variants endpoint is also
   * what lets a visitor without a token see the sizes at all: that endpoint is
   * behind the `User,Admin` roles, and the product itself is not.
   */
  readonly variants = computed<readonly ProductVariant[]>(
    () => this.product()?.variants.filter((variant) => variant.isActive) ?? [],
  );

  /** A product nobody has opened a sellable variant for yet. */
  readonly hasNoVariants = computed(() => this.product() !== null && !this.variants().length);

  /**
   * The variant the shopper has chosen. Defaults to the first one offered, so the
   * page is buyable the moment it settles, and holds that choice across a refetch
   * as long as the variant is still on sale.
   */
  readonly selectedVariantId = linkedSignal<readonly ProductVariant[], string>({
    source: this.variants,
    computation: (variants, previous) => {
      const kept = previous?.value && variants.some((variant) => variant.id === previous.value);
      return kept ? previous.value : (variants[0]?.id ?? '');
    },
  });

  readonly selectedVariant = computed<ProductVariant | null>(
    () => this.variants().find((variant) => variant.id === this.selectedVariantId()) ?? null,
  );

  /**
   * Back to one whenever the shopper switches variant: a count chosen for a 30ml
   * bottle is not a statement about the 100ml one.
   */
  readonly quantity = linkedSignal<string, number>({
    source: this.selectedVariantId,
    computation: () => 1,
  });

  /**
   * A variant sells at its own price, or at its product's when it has none. Null
   * when neither carries one, which is what the API refuses to cart.
   */
  readonly unitPrice = computed<number | null>(
    () => this.selectedVariant()?.price ?? this.product()?.price ?? null,
  );

  readonly lineTotal = computed(() => (this.unitPrice() ?? 0) * this.quantity());

  readonly isAdding = this.cart.isAdding;

  readonly canAddToCart = computed(
    () => this.isSignedIn() && this.selectedVariant() !== null && !this.cart.isBusy(),
  );

  /** What the confirmation calls what was just added. */
  readonly selectionLabel = computed(() => {
    const variant = this.selectedVariant();
    if (!variant) {
      return '';
    }

    const name = this.product()?.name ?? variant.productName ?? 'Item';
    const quantity = this.quantity();
    const label = `${name} (${variant.sku})`;

    return quantity > 1 ? `${quantity} x ${label}` : label;
  });

  selectVariant(variantId: string): void {
    this.selectedVariantId.set(variantId);
  }

  /** Clamped here rather than trusted from the input, which can be typed into. */
  setQuantity(quantity: number): void {
    const whole = Math.trunc(Number.isFinite(quantity) ? quantity : 1);
    this.quantity.set(Math.min(Math.max(1, whole), MAX_QUANTITY));
  }

  stepQuantity(by: number): void {
    this.setQuantity(this.quantity() + by);
  }

  /**
   * Sends the choice to the cart. The store announces it and updates the badge;
   * anything the API refused - a variant taken off sale between the page loading
   * and the button being pressed - is reported by `errorInterceptor`.
   */
  addToCart(): void {
    const variant = this.selectedVariant();
    if (!variant || !this.canAddToCart()) {
      return;
    }

    this.cart.add(variant.id, this.quantity(), this.selectionLabel()).subscribe();
  }

  reload(): void {
    this.productResource.reload();
  }
}
