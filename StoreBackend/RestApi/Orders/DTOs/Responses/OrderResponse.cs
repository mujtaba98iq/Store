using RestApi.Payments;
using RestApi.Shipments;

namespace RestApi.Orders;

public class OrderResponse
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string OrderNumber { get; set; }

    /// <summary>
    /// Rendered by name rather than by number, so a client never has to carry a copy of
    /// the enum to make sense of it.
    /// </summary>
    public required string Status { get; set; }

    public required List<OrderItemResponse> Items { get; set; }

    /// <summary>
    /// The address this order was sent to, as it read at checkout. Null only on an order
    /// placed before addresses were recorded.
    /// </summary>
    public OrderShippingAddressResponse? ShippingAddress { get; set; }

    /// <summary>
    /// Every attempt to settle this order, oldest first. More than one is normal: a decline
    /// and the retry that followed it both show, which is what makes the history readable.
    /// </summary>
    public required List<PaymentResponse> Payments { get; set; }

    /// <summary>
    /// The parcel. Null on an order placed before shipments were tracked.
    /// </summary>
    public ShipmentResponse? Shipment { get; set; }

    /// <summary>
    /// Number of lines on the order, not the number of units across them.
    /// </summary>
    public required int ItemCount { get; set; }

    public required decimal Subtotal { get; set; }
    public required decimal DiscountAmount { get; set; }
    public required decimal ShippingAmount { get; set; }
    public required decimal TotalAmount { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
