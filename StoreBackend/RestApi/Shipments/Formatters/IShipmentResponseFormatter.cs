using Domain.Shipments;

namespace RestApi.Shipments;

public interface IShipmentResponseFormatter
{
    ShipmentResponse One(Shipment shipment);
    ShipmentListResponse Many(IEnumerable<Shipment> shipments, int totalCount);
}
