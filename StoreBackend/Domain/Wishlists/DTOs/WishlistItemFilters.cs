using Sheard.Type;

namespace Domain.Wishlists;

public class WishlistItemFilters : ListingOptions
{
    public Guid? WishlistItemId { get; set; }

    /// <summary>
    /// Whose list to read. It is what turns this into "a wishlist" rather than a listing of
    /// every entry in the table, and every customer-facing route sets it.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Read across customers, this answers which products are wanted most, the entries for
    /// one product being the count of people waiting on it.
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Bounds on when the product was added. Inclusive at both ends.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public WishlistItemOrderBy? OrderBy { get; set; } = WishlistItemOrderBy.CreatedAt;
}
