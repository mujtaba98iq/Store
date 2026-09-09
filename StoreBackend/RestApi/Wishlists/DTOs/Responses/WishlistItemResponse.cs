namespace RestApi.Wishlists;

public class WishlistItemResponse
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string ProductId { get; set; }

    /// <summary>
    /// When the product was added, which is what the list is ordered by and the only thing
    /// a client can sort on.
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    public required string CreatedById { get; set; }
}
