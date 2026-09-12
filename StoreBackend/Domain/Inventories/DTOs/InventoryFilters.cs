using Sheard.Type;

namespace Domain.Inventories;

public class InventoryFilters : ListingOptions
{
    public Guid? InventoryId { get; set; }
    public Guid? ProductVariantId { get; set; }

    /// <summary>
    /// Partial match against the SKU of the variant the row stocks. A stock row has
    /// no name of its own, so this is what a reader searches an inventory listing by.
    /// </summary>
    public string? Sku { get; set; }

    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }

    /// <summary>
    /// True keeps only rows that still have units left to sell, false keeps only the sold out ones.
    /// </summary>
    public bool? IsAvailable { get; set; }

    public InventoryOrderBy? OrderBy { get; set; } = InventoryOrderBy.CreatedAt;
}
