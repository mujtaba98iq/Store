/**
 * The part of `RestApi.ProductVariants.ProductVariantResponse` the stock form needs:
 * enough to pick one variant out of a list and to say which it was.
 */
export interface ProductVariant {
  readonly id: string;
  readonly sku: string;
  readonly isActive: boolean;
}
