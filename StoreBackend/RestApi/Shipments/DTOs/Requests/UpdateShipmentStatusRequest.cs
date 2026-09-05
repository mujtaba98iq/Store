using Domain.Shipments;

namespace RestApi.Shipments;

public class UpdateShipmentStatusRequest
{
    public required ShipmentStatus Status { get; set; }
}
