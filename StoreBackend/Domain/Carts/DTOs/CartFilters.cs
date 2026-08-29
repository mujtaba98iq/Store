using Sheard.Type;

namespace Domain.Carts;

public class CartFilters : ListingOptions
{
    public Guid? CartId { get; set; }
    public Guid? UserId { get; set; }

    /// <summary>
    /// True keeps only the carts nothing has been added to, false keeps only the ones
    /// holding at least one line.
    /// </summary>
    public bool? IsEmpty { get; set; }

    public CartOrderBy? OrderBy { get; set; } = CartOrderBy.CreatedAt;
}
