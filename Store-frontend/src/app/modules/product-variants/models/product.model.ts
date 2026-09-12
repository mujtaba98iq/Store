/**
 * The part of `RestApi.Products.ProductResponse` this page needs: enough to pick a
 * product out of a list, and to narrow the listing to one.
 *
 * Products themselves are managed in their own slice; nothing here ever writes one.
 */
export interface ProductOption {
  readonly id: string;
  readonly name: string;
}
