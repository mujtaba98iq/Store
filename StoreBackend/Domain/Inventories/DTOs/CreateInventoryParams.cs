namespace Domain.Inventories;

public class CreateInventoryParams
{
    public required Guid ProductVariantId { get; set; }
    public int? Quantity { get; set; }
    public int? ReservedQuantity { get; set; }
    public required string CreatedById { get; set; }
}
