namespace RestApi.Inventories;

public class CreateInventoryRequest
{
    public required Guid ProductVariantId { get; set; }
    public int? Quantity { get; set; }
    public int? ReservedQuantity { get; set; }
}
