using Domain.Coupons;
using Domain.Data;
using Domain.Payments;
using Domain.Shipments;
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
    /// Money off the order as a whole, worked out from <see cref="CouponId"/> at checkout.
    /// Discounts that belong to a single line sit on that line instead and are already
    /// inside <see cref="Subtotal"/>.
    ///
    /// Frozen like the rest of the money: editing the coupon afterwards, or withdrawing it
    /// altogether, does not change what this order was billed.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// The campaign the discount came from, where one was quoted. Null on an order placed
    /// without a coupon, which is most of them.
    /// </summary>
    public Guid? CouponId { get; set; }

    /// <summary>
    /// The code as it was redeemed, copied here for the same reason the product name and SKU
    /// are copied onto a line: the order has to stay readable on its own. A coupon row can be
    /// edited or deleted years later, and this is what still says what the customer typed.
    /// </summary>
    public string? CouponCode { get; set; }

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

    /// <summary>
    /// The campaign behind <see cref="DiscountAmount"/>. Null where none was quoted, and
    /// also null unless a read asked for it, which the order's own reads do not: everything
    /// an order needs to say about its discount is already on the order.
    /// </summary>
    public Coupon? Coupon { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// The address the order was sent to, frozen at checkout in the same way its lines and
    /// its money are. Nullable only because a read may not have asked for it.
    /// </summary>
    public OrderShippingAddress? ShippingAddress { get; set; }

    /// <summary>
    /// Every attempt to settle this order. More than one is normal rather than a fault: a
    /// decline and the retry that followed it are both part of how the order got paid.
    /// Empty unless a read asked for them, which the order's own reads do not.
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// The parcel, once there is one. Nullable because an order is placed well before it
    /// ships, and because a read may not have asked for it.
    /// </summary>
    public Shipment? Shipment { get; set; }
}
