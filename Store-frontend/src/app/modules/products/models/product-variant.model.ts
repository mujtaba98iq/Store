/**
 * Mirrors `RestApi.ProductVariants.ProductVariantResponse` as the product carries
 * it.
 *
 * One sellable form of a product - a size, a shade - identified by its own SKU.
 * It is what a cart line actually points at; the product above it is only the
 * thing being varied.
 */
export interface ProductVariant {
  readonly id: string;
  readonly productId: string;
  /** Null only if the product behind the variant is gone. */
  readonly productName: string | null;
  readonly sku: string;
  /** Null means the variant has no price of its own and sells at the product's. */
  readonly price: number | null;
  readonly barcode: string | null;
  /** An inactive variant stays on the books but the API refuses to cart it. */
  readonly isActive: boolean;
}
