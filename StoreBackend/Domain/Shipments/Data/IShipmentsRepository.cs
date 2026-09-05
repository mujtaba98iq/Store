namespace Domain.Shipments;

public interface IShipmentsRepository
{
    Task<Shipment> Create(Shipment shipment);
    Task<Shipment> Update(Shipment shipment);
    Task<Shipment?> FindById(Guid id);

    /// <summary>
    /// The parcel for an order. At most one, which the unique key on OrderId guarantees.
    /// </summary>
    Task<Shipment?> FindByOrderId(Guid orderId);

    Task<List<Shipment>> FindByFilters(ShipmentFilters shipmentFilters);
    Task<int> GetTotalCountByFilters(ShipmentFilters shipmentFilters);
}
