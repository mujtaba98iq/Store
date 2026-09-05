namespace Domain.Shipments;

public class UpdateShipmentParams
{
    public required Guid ShipmentId { get; set; }

    /// <summary>
    /// Left as they are when null, so the carrier can be set in one call and its number
    /// filled in by another once the label comes back.
    /// </summary>
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }

    public required string UpdatedById { get; set; }
}
