namespace RestApi.Inventories;

public class InventoryResponse
{
    public required string Id { get; set; }
    public required string ProductVariantId { get; set; }

    /// <summary>
    /// The SKU of the variant this row stocks. A stock row has no name of its own, so
    /// without it a client has nothing to label the row with but a GUID.
    /// </summary>
    public string? Sku { get; set; }

    public required int Quantity { get; set; }
    public required int ReservedQuantity { get; set; }

    /// <summary>
    /// Quantity - ReservedQuantity. Exposed so clients never have to compute it themselves.
    /// </summary>
    public required int AvailableQuantity { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
