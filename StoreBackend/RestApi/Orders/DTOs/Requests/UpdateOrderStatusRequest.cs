using Domain.Orders;

namespace RestApi.Orders;

public class UpdateOrderStatusRequest
{
    public required OrderStatus Status { get; set; }
}
