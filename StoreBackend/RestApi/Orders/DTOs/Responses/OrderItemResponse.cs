namespace RestApi.Orders;

public class OrderItemResponse
{
    public required string Id { get; set; }
    public required string OrderId { get; set; }

    /// <summary>
    /// Reference back to the catalogue. The fields below are the copies the order was
    /// actually placed against, so a client should render those and not re-fetch this.
    /// </summary>
    public required string ProductVariantId { get; set; }

    /// <summary>
    /// Product name and SKU as they read at checkout, not as they read today.
    /// </summary>
    public required string ProductName { get; set; }
    public required string Sku { get; set; }

    /// <summary>
    /// What one unit was bought for, not what it costs now.
    /// </summary>
    public required decimal UnitPrice { get; set; }

    public required int Quantity { get; set; }

    /// <summary>
    /// Money taken off this line specifically, separate from any order-wide discount.
    /// </summary>
    public required decimal DiscountAmount { get; set; }

    /// <summary>
    /// Quantity * UnitPrice - DiscountAmount, as charged.
    /// </summary>
    public required decimal TotalAmount { get; set; }

    public required DateTime CreatedAt { get; set; }
}
