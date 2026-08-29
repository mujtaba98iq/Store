namespace Domain.Carts;

public class GetOrCreateCartParams
{
    public required Guid UserId { get; set; }
    public required string CreatedById { get; set; }
}
