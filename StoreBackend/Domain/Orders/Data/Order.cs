using Domain.Data;
using Domain.Users;

namespace Domain.Orders;

/// <summary>
/// A checkout that has been committed. Unlike a cart, an order is a historical record:
/// its lines and its money are frozen at the moment it was placed, so a later price
/// change or a renamed product can never rewrite what the customer actually agreed to.
/// </summary>
public class Order : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// The reference a customer quotes when they get in touch. Unique, and separate from
    /// the id so it can be read out loud without exposing a key.
    /// </summary>
    public required string OrderNumber { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Sum of the line totals, each already net of whatever was taken off that line. Stored
    /// rather than derived: the lines are frozen, but so is this, and recomputing it would
    /// quietly hide any drift between the two.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Money off the order as a whole, such as a coupon. Discounts that belong to a single
    /// line sit on that line instead and are already inside <see cref="Subtotal"/>.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Subtotal - DiscountAmount + ShippingAmount, as charged. Stored for the same reason
    /// as Subtotal: this is the number the customer was billed, not a number to re-derive.
    /// </summary>
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// The address the order was sent to, frozen at checkout in the same way its lines and
    /// its money are. Nullable only because a read may not have asked for it.
    /// </summary>
    public OrderShippingAddress? ShippingAddress { get; set; }
}
