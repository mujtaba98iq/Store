namespace Domain.Orders;

public interface IOrderItemsRepository
{
    Task<OrderItem?> FindById(Guid id);
    Task<List<OrderItem>> FindByOrderId(Guid orderId);

    /// <summary>
    /// Whether one of an order's lines was for the given product. Lines name variants, so
    /// the question has to be answered through the variant each one points at.
    ///
    /// Asked when a customer reviews something they bought: the order is the proof.
    /// </summary>
    Task<bool> ContainsProduct(Guid orderId, Guid productId);

    /// <summary>
    /// Writes every line in one round trip, so an order can never be left holding only
    /// part of what was checked out.
    /// </summary>
    Task<List<OrderItem>> CreateMany(List<OrderItem> orderItems);
}
