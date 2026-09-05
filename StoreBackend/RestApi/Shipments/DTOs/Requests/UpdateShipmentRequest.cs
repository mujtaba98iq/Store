namespace RestApi.Shipments;

public class UpdateShipmentRequest
{
    /// <summary>
    /// Either may be left out, and what is left out stays as it was: the carrier can be
    /// recorded in one call and its number added by another once the label is printed.
    /// </summary>
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }
}
