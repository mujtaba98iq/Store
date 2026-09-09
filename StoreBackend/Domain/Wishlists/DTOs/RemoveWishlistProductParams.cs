namespace Domain.Wishlists;

/// <summary>
/// Removal addressed by product rather than by entry id, which is what a product page has
/// to work with: the heart there knows what it is showing, not which row put it on the list.
/// </summary>
public class RemoveWishlistProductParams
{
    public required Guid UserId { get; set; }
    public required Guid ProductId { get; set; }
    public required string DeletedById { get; set; }
}
