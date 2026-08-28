namespace RestApi.Inventories;

public class InventoryResponse
{
    public required string Id { get; set; }
    public required string ProductVariantId { get; set; }
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
