import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductDetailsStore } from '../../data-access/product-details-store';
import { ProductVariant } from '../../models/product-variant.model';
import { productImageUrl } from '../../utils/product-image';

/**
 * One product, and the two choices that turn it into a cart line: which variant,
 * and how many. The variants are what is actually sold - the product above them is
 * only the thing being varied - so the page cannot be bought from until one is
 * picked, and it picks the first for the shopper.
 */
@Component({
  selector: 'app-product-details',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './product-details.html',
  styleUrl: './product-details.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProductDetailsStore],
})
export class ProductDetails {
  private readonly store = inject(ProductDetailsStore);

  protected readonly product = this.store.product;
  protected readonly isLoading = this.store.isLoading;
  protected readonly errorMessage = this.store.errorMessage;

  protected readonly isSignedIn = this.store.isSignedIn;
  protected readonly variants = this.store.variants;
  protected readonly hasNoVariants = this.store.hasNoVariants;

  protected readonly selectedVariantId = this.store.selectedVariantId;
  protected readonly quantity = this.store.quantity;
  protected readonly maxQuantity = this.store.maxQuantity;
  protected readonly unitPrice = this.store.unitPrice;
  protected readonly lineTotal = this.store.lineTotal;
  protected readonly canAddToCart = this.store.canAddToCart;
  protected readonly isAdding = this.store.isAdding;

  /** Uploaded image first, then the legacy imagePath. */
  protected readonly image = computed(() => {
    const product = this.product();
    return product ? productImageUrl(product) : '';
  });

  protected readonly categoryName = computed(() => this.product()?.categories[0]?.name ?? '');

  /** Where signing in should come back to, so the choice is not lost. */
  protected readonly returnUrl = computed(() => `/products/${this.store.productId()}`);

  /**
   * A variant is labelled by its SKU - it is the only name one has - with its own
   * price shown when it differs from the product's.
   */
  protected priceOf(variant: ProductVariant): number | null {
    return variant.price ?? this.product()?.price ?? null;
  }

  protected selectVariant(variantId: string): void {
    this.store.selectVariant(variantId);
  }

  /**
   * The store clamps what was typed, and a value that clamps back to the one
   * already held leaves the binding with nothing to redraw - so the box is reset
   * first and the binding overwrites it whenever the count really did move.
   */
  protected onQuantityInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const typed = Number(input.value);

    input.value = String(this.quantity());
    this.store.setQuantity(typed);
  }

  protected stepQuantity(by: number): void {
    this.store.stepQuantity(by);
  }

  protected addToCart(): void {
    this.store.addToCart();
  }

  protected retry(): void {
    this.store.reload();
  }
}
