namespace Domain.Carts;

public class ClearCartParams
{
    public required Guid UserId { get; set; }
    public required string DeletedById { get; set; }
}
