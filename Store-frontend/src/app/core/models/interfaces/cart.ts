/**
 * Mirrors `RestApi.Carts.CartItemResponse`.
 *
 * One line of the cart. It points at a product variant rather than a product - a
 * size is what is actually bought - and it carries the price the variant sold for
 * when the line was last touched, not a live one.
 */
export interface CartItem {
  readonly id: string;
  readonly cartId: string;
  readonly productVariantId: string;
  readonly quantity: number;
  readonly unitPrice: number;
  /** `quantity * unitPrice`, computed by the API rather than here. */
  readonly subtotal: number;
  readonly createdAt: string;
  readonly updatedAt: string | null;
}

/**
 * Mirrors `RestApi.Carts.CartResponse`.
 *
 * Every cart endpoint answers with the whole cart, so a write never has to be
 * followed by a read - see `CartStore`, which just replaces what it holds.
 */
export interface Cart {
  readonly id: string;
  readonly userId: string;
  readonly items: readonly CartItem[];
  /**
   * Lines in the cart, not units across them. `CartStore.unitCount` sums the
   * units, which is what the header badge counts.
   */
  readonly itemCount: number;
  /** Sum of every line subtotal. */
  readonly totalAmount: number;
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly createdById: string;
  readonly updatedById: string | null;
}

/**
 * Mirrors `RestApi.Carts.AddCartItemRequest`.
 *
 * Adding a variant the cart already holds tops up that line rather than opening a
 * second one, so this quantity is always a delta.
 */
export interface AddCartItemBody {
  readonly productVariantId: string;
  readonly quantity: number;
}

/**
 * Mirrors `RestApi.Carts.UpdateCartItemRequest`.
 *
 * Sets the quantity of a line rather than adding to it, so a retry of the same
 * call cannot double what the shopper asked for. Zero is refused: emptying a line
 * is what the delete is for.
 */
export interface UpdateCartItemBody {
  readonly quantity: number;
}
