namespace RestApi.Shipments;

public class CreateShipmentRequest
{
    /// <summary>
    /// The order to open a parcel for. Only needed for an order placed before parcels were
    /// tracked: checkout opens one for everything since.
    /// </summary>
    public required Guid OrderId { get; set; }

    /// <summary>
    /// Optional at this point. A parcel starts out queued for the warehouse, with the
    /// carrier settled on later.
    /// </summary>
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }
}
