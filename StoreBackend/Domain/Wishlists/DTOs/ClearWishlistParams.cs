namespace Domain.Wishlists;

public class ClearWishlistParams
{
    public required Guid UserId { get; set; }
    public required string DeletedById { get; set; }
}
