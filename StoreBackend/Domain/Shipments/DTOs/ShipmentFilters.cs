using Sheard.Type;

namespace Domain.Shipments;

public class ShipmentFilters : ListingOptions
{
    public Guid? ShipmentId { get; set; }
    public Guid? OrderId { get; set; }

    /// <summary>
    /// The customer whose order this parcel belongs to. Shipments carry no user of their
    /// own, so this is read through the order.
    /// </summary>
    public Guid? UserId { get; set; }

    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }
    public ShipmentStatus? Status { get; set; }

    /// <summary>
    /// Bounds on when the parcel was opened. Inclusive at both ends.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public ShipmentOrderBy? OrderBy { get; set; } = ShipmentOrderBy.CreatedAt;
}
