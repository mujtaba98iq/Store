namespace Domain.Carts;

public class AddCartItemParams
{
    public required Guid UserId { get; set; }
    public required Guid ProductVariantId { get; set; }
    public required int Quantity { get; set; }
    public required string CreatedById { get; set; }
}
