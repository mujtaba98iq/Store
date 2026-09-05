using Sheard.Type;

namespace Domain.Shipments;

public interface IShipmentService
{
    /// <summary>
    /// Opens the parcel for an order. Refuses a second one, because two of them could not
    /// agree on where the order has got to.
    /// </summary>
    Task<Shipment> Create(CreateShipmentParams createShipmentParams);

    Task<Shipment?> FindById(Guid id);
    Task<Shipment?> FindByOrderId(Guid orderId);
    Task<PaginationResult<Shipment>> Search(ShipmentFilters shipmentFilters);

    /// <summary>
    /// Records who is carrying the parcel and under what number.
    /// </summary>
    Task<Shipment> Update(UpdateShipmentParams updateShipmentParams);

    /// <summary>
    /// Moves the parcel along its own lifecycle: picked, dispatched, out for delivery,
    /// delivered or returned.
    /// </summary>
    Task<Shipment> UpdateStatus(UpdateShipmentStatusParams updateShipmentStatusParams);

    /// <summary>
    /// Brings the parcel into line with an order that has just moved, opening one first if
    /// the order predates shipment tracking. Called by the order rather than by a client,
    /// and deliberately quiet: a parcel that is already further along than the order is left
    /// where it is instead of refusing the order's own transition.
    /// </summary>
    Task SyncWithOrderStatus(Guid orderId, ShipmentStatus status, string updatedById);
}
