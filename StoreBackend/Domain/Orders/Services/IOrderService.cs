using Sheard.Type;

namespace Domain.Orders;

public interface IOrderService
{
    /// <summary>
    /// Turns the customer's cart into an order: copies the lines at the prices they were
    /// shown, reserves the stock, and empties the cart.
    /// </summary>
    Task<Order> Checkout(CheckoutParams checkoutParams);

    Task<Order?> FindById(Guid id);
    Task<PaginationResult<Order>> Search(OrderFilters orderFilters);

    /// <summary>
    /// Moves an order along its lifecycle. Staff-facing: any transition the order allows.
    /// </summary>
    Task<Order> UpdateStatus(UpdateOrderStatusParams updateOrderStatusParams);

    /// <summary>
    /// Customer-facing cancellation, limited to the caller's own order.
    /// </summary>
    Task<Order> Cancel(CancelOrderParams cancelOrderParams);
}
