namespace Domain.Inventories;

public class UpdateInventoryParams
{
    public required Guid Id { get; set; }
    public int? Quantity { get; set; }
    public int? ReservedQuantity { get; set; }
    public required string UpdatedById { get; set; }
}
