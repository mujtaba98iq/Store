/**
 * The part of `RestApi.ProductVariants.ProductVariantResponse` a cart line needs to
 * be labelled: what it is called, what code it carries, and which product page it
 * came from.
 *
 * A cart line only stores the variant's id - the API carries no more than that on
 * the line itself - so this is what turns a row of ids into a row a shopper can
 * read.
 */
export interface ProductVariant {
  readonly id: string;
  readonly productId: string;
  /** Null only if the product behind the variant is gone. */
  readonly productName: string | null;
  readonly sku: string;
}
