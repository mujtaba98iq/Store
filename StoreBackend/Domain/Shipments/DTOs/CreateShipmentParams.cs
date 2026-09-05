namespace Domain.Shipments;

public class CreateShipmentParams
{
    public required Guid OrderId { get; set; }

    /// <summary>
    /// Both optional at this point. A parcel is opened when the order is placed, long
    /// before anyone has decided who will carry it.
    /// </summary>
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }

    public required string CreatedById { get; set; }
}
