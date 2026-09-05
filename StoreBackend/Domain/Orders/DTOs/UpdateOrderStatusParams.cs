namespace Domain.Orders;

public class UpdateOrderStatusParams
{
    public required Guid OrderId { get; set; }
    public required OrderStatus Status { get; set; }
    public required string UpdatedById { get; set; }
}
