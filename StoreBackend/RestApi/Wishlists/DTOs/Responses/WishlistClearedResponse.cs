namespace RestApi.Wishlists;

public class WishlistClearedResponse
{
    /// <summary>
    /// How many entries came off. Zero where the list was already empty, that being no
    /// error: the caller asked for an empty wishlist and an empty wishlist is what they have.
    /// </summary>
    public required int RemovedCount { get; set; }
}
