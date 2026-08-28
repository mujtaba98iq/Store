namespace RestApi.Inventories;

public class UpdateInventoryRequest
{
    public int? Quantity { get; set; }
    public int? ReservedQuantity { get; set; }
}
