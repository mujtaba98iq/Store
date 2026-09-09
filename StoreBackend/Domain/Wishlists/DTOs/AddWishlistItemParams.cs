namespace Domain.Wishlists;

public class AddWishlistItemParams
{
    /// <summary>
    /// Whose list the product goes on. Kept apart from <see cref="CreatedById"/> so the two
    /// stay honest if staff ever add to a customer's list on their behalf.
    /// </summary>
    public required Guid UserId { get; set; }

    public required Guid ProductId { get; set; }

    public required string CreatedById { get; set; }
}
