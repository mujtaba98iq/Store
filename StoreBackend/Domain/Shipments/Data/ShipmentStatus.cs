namespace Domain.Shipments;

/// <summary>
/// Where a parcel has got to. The values are persisted as numbers, so they may be added to
/// but never renumbered.
/// </summary>
public enum ShipmentStatus
{
    /// <summary>
    /// Queued for the warehouse. Nothing has been picked yet.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Being picked and packed.
    /// </summary>
    Preparing = 2,

    /// <summary>
    /// Handed to the carrier and on its way.
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// On the courier's round for the day, which is the point at which a customer expects a
    /// knock at the door.
    /// </summary>
    OutForDelivery = 4,

    Delivered = 5,

    /// <summary>
    /// Came back: refused at the door, undeliverable, or sent back by the customer.
    /// </summary>
    Returned = 6,
}
