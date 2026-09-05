namespace Domain.Orders;

/// <summary>
/// Where an order has got to. The values are persisted as numbers, so they may be added
/// to but never renumbered.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Placed by the customer, not yet accepted. Stock is reserved from this point on.
    /// </summary>
    Pending = 1,

    Confirmed = 2,
    Processing = 3,

    /// <summary>
    /// Handed to the carrier. The reservation becomes a real deduction here, because the
    /// units have physically left.
    /// </summary>
    Shipped = 4,

    Delivered = 5,

    /// <summary>
    /// Called off before shipping. Releases whatever was reserved.
    /// </summary>
    Cancelled = 6,
}
