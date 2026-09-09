namespace RestApi.Wishlists;

public class AddWishlistItemRequest
{
    /// <summary>
    /// The product to keep hold of. No variant, and no quantity: neither is decided until
    /// the customer moves the product to their cart.
    /// </summary>
    public required Guid ProductId { get; set; }
}
