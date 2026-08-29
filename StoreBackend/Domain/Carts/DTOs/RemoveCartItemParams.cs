namespace Domain.Carts;

public class RemoveCartItemParams
{
    public required Guid UserId { get; set; }
    public required Guid CartItemId { get; set; }
    public required string DeletedById { get; set; }
}
