namespace Domain.Inventories;

public class DeleteInventoryParams
{
    public required Guid Id { get; set; }
    public required string DeletedById { get; set; }
}
