namespace Domain.Shipments;

public class UpdateShipmentStatusParams
{
    public required Guid ShipmentId { get; set; }
    public required ShipmentStatus Status { get; set; }
    public required string UpdatedById { get; set; }
}
