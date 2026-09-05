using Domain.Shipments;

namespace RestApi.Shipments;

public class ShipmentResponseFormatter : IShipmentResponseFormatter
{
    public ShipmentListResponse Many(IEnumerable<Shipment> shipments, int totalCount)
    {
        return new ShipmentListResponse
        {
            Data = shipments.Select(One).ToList(),
            TotalCount = totalCount
        };
    }

    public ShipmentResponse One(Shipment shipment)
    {
        return new ShipmentResponse
        {
            Id = shipment.Id.ToString(),
            OrderId = shipment.OrderId.ToString(),
            TrackingNumber = shipment.TrackingNumber,
            ShippingProvider = shipment.ShippingProvider,
            Status = shipment.Status.ToString(),
            ShippedAt = shipment.ShippedAt,
            DeliveredAt = shipment.DeliveredAt,
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt,
            CreatedById = shipment.CreatedById,
            UpdatedById = shipment.UpdatedById
        };
    }
}
