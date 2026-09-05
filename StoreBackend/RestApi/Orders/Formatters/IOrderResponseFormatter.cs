using Domain.Orders;

namespace RestApi.Orders;

public interface IOrderResponseFormatter
{
    OrderResponse One(Order order);
    OrderListResponse Many(IEnumerable<Order> orders, int totalCount);
}
