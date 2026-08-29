namespace Domain.Carts;

public class UpdateCartItemParams
{
    public required Guid UserId { get; set; }
    public required Guid CartItemId { get; set; }
    public required int Quantity { get; set; }
    public required string UpdatedById { get; set; }
}
