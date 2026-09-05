namespace RestApi.Shipments;

public class ShipmentResponse
{
    public required string Id { get; set; }
    public required string OrderId { get; set; }

    /// <summary>
    /// Both null until the parcel is handed to a carrier: until then there is nobody to
    /// chase and no number to chase them with.
    /// </summary>
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }

    /// <summary>
    /// Rendered by name rather than by number, so a client never has to carry a copy of
    /// the enum to make sense of it.
    /// </summary>
    public required string Status { get; set; }

    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
