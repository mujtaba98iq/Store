namespace Domain.Orders;

public interface IOrderItemsRepository
{
    Task<OrderItem?> FindById(Guid id);
    Task<List<OrderItem>> FindByOrderId(Guid orderId);

    /// <summary>
    /// Writes every line in one round trip, so an order can never be left holding only
    /// part of what was checked out.
    /// </summary>
    Task<List<OrderItem>> CreateMany(List<OrderItem> orderItems);
}
