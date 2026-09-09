namespace Domain.Wishlists;

public class RemoveWishlistItemParams
{
    public required Guid WishlistItemId { get; set; }

    /// <summary>
    /// The list the entry has to be on. An entry belonging to somebody else is reported as
    /// missing, so a stranger cannot learn what is on it by trying to remove things from it.
    /// </summary>
    public required Guid UserId { get; set; }

    public required string DeletedById { get; set; }
}
