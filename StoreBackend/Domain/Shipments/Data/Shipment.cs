using Domain.Data;
using Domain.Orders;

namespace Domain.Shipments;

/// <summary>
/// The parcel side of an order: who is carrying it, under what number, and how far along it
/// is. Kept apart from the order's own status because the two answer different questions.
/// An order is <see cref="OrderStatus.Shipped"/> the moment it leaves the shop and stops
/// caring after that; the shipment goes on to say whether it is out for delivery today,
/// was delivered, or came back.
///
/// One per order, which the one-to-one enforces. Splitting an order across several parcels
/// would mean relaxing that and then deciding what the order's own status says when its
/// parcels disagree.
/// </summary>
public class Shipment : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>
    /// What the customer quotes to the carrier to chase the parcel. Null until it is
    /// actually handed over: a shipment still being picked has nothing to track yet.
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Who is carrying it. Null for the same reason as the tracking number — the carrier is
    /// settled on at dispatch, not when the order is placed.
    /// </summary>
    public string? ShippingProvider { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    /// <summary>
    /// When the parcel left. Null until it does.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// When it reached the customer. Null until it does. A parcel that came back without
    /// ever arriving keeps it null; one returned after arriving keeps the day it arrived,
    /// because it did arrive.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Order? Order { get; set; }
}
