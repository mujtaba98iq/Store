using Domain.Exeptions;
using Domain.Orders;
using Sheard.Type;

namespace Domain.Shipments
{
    public class ShipmentService(
        IShipmentsRepository shipmentsRepository,
        IOrdersRepository ordersRepository) : IShipmentService
    {
        /// <summary>
        /// Where a parcel may go next. Preparing can be skipped, because a shop that picks
        /// and hands over in one motion should not have to record a step it never took.
        /// Delivered can still go to Returned: a customer sending something back is a
        /// return, and the parcel is the only thing that can say so.
        /// </summary>
        private static readonly Dictionary<ShipmentStatus, ShipmentStatus[]> AllowedTransitions = new()
        {
            [ShipmentStatus.Pending] = [ShipmentStatus.Preparing, ShipmentStatus.Shipped],
            [ShipmentStatus.Preparing] = [ShipmentStatus.Shipped],
            [ShipmentStatus.Shipped] = [ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered, ShipmentStatus.Returned],
            [ShipmentStatus.OutForDelivery] = [ShipmentStatus.Delivered, ShipmentStatus.Returned],
            [ShipmentStatus.Delivered] = [ShipmentStatus.Returned],
            [ShipmentStatus.Returned] = [],
        };

        public async Task<Shipment> Create(CreateShipmentParams createShipmentParams)
        {
            var order = await FindOrderOrThrow(createShipmentParams.OrderId);

            if (await shipmentsRepository.FindByOrderId(order.Id) != null)
            {
                throw new ShipmentAlreadyExistsException($"Order {order.OrderNumber} already has a shipment.");
            }

            var shipment = await shipmentsRepository.Create(new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                TrackingNumber = createShipmentParams.TrackingNumber,
                ShippingProvider = createShipmentParams.ShippingProvider,
                Status = ShipmentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createShipmentParams.CreatedById
            });

            return await shipmentsRepository.FindById(shipment.Id) ?? shipment;
        }

        public async Task<Shipment?> FindById(Guid id)
        {
            return await shipmentsRepository.FindById(id);
        }

        public async Task<Shipment?> FindByOrderId(Guid orderId)
        {
            return await shipmentsRepository.FindByOrderId(orderId);
        }

        public async Task<PaginationResult<Shipment>> Search(ShipmentFilters shipmentFilters)
        {
            var shipments = await shipmentsRepository.FindByFilters(shipmentFilters);
            var totalCount = await shipmentsRepository.GetTotalCountByFilters(shipmentFilters);

            return new PaginationResult<Shipment>
            {
                TotalCount = totalCount,
                Data = shipments
            };
        }

        public async Task<Shipment> Update(UpdateShipmentParams updateShipmentParams)
        {
            var shipment = await FindOrThrow(updateShipmentParams.ShipmentId);

            // A null leaves the value alone rather than clearing it, so the carrier can be
            // recorded in one call and its number added by another once the label is printed.
            shipment.TrackingNumber = updateShipmentParams.TrackingNumber ?? shipment.TrackingNumber;
            shipment.ShippingProvider = updateShipmentParams.ShippingProvider ?? shipment.ShippingProvider;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedById = updateShipmentParams.UpdatedById;

            await shipmentsRepository.Update(shipment);

            return await shipmentsRepository.FindById(shipment.Id) ?? shipment;
        }

        public async Task<Shipment> UpdateStatus(UpdateShipmentStatusParams updateShipmentStatusParams)
        {
            var shipment = await FindOrThrow(updateShipmentStatusParams.ShipmentId);

            EnsureTransitionIsAllowed(shipment.Status, updateShipmentStatusParams.Status);

            return await Transition(shipment, updateShipmentStatusParams.Status, updateShipmentStatusParams.UpdatedById);
        }

        public async Task SyncWithOrderStatus(Guid orderId, ShipmentStatus status, string updatedById)
        {
            // Opened on the spot for an order placed before parcels were tracked. A newer
            // order already has one from checkout, so this is a fallback rather than the
            // usual path.
            var shipment = await shipmentsRepository.FindByOrderId(orderId)
                           ?? await Create(new CreateShipmentParams
                           {
                               OrderId = orderId,
                               CreatedById = updatedById
                           });

            // Quiet on purpose. A parcel already at or past where the order has reached is
            // left alone: the order moving is the fact being recorded, and refusing it
            // because its parcel is ahead would be the follower overruling the record.
            if (shipment.Status == status || !IsTransitionAllowed(shipment.Status, status))
            {
                return;
            }

            await Transition(shipment, status, updatedById);
        }

        private async Task<Shipment> Transition(Shipment shipment, ShipmentStatus status, string updatedById)
        {
            var now = DateTime.UtcNow;

            shipment.Status = status;
            shipment.UpdatedAt = now;
            shipment.UpdatedById = updatedById;

            // Both are stamped once and then left. A parcel that comes back keeps the day it
            // went out, and one returned after arriving keeps the day it arrived: neither
            // stops having happened.
            switch (status)
            {
                case ShipmentStatus.Shipped:
                    shipment.ShippedAt ??= now;
                    break;
                case ShipmentStatus.Delivered:
                    shipment.DeliveredAt ??= now;
                    break;
            }

            await shipmentsRepository.Update(shipment);

            return await shipmentsRepository.FindById(shipment.Id) ?? shipment;
        }

        private async Task<Shipment> FindOrThrow(Guid id)
        {
            return await shipmentsRepository.FindById(id)
                   ?? throw new ResourceNotFoundException("Shipment", $"Shipment with ID {id} not found");
        }

        private async Task<Order> FindOrderOrThrow(Guid orderId)
        {
            return await ordersRepository.FindById(orderId)
                   ?? throw new ResourceNotFoundException("Order", $"Order with ID {orderId} not found");
        }

        private static void EnsureTransitionIsAllowed(ShipmentStatus current, ShipmentStatus next)
        {
            if (current == next)
            {
                throw new InvalidShipmentStatusTransitionException($"Shipment is already {current}.");
            }

            if (!IsTransitionAllowed(current, next))
            {
                throw new InvalidShipmentStatusTransitionException($"A shipment cannot move from {current} to {next}.");
            }
        }

        private static bool IsTransitionAllowed(ShipmentStatus current, ShipmentStatus next)
        {
            var allowed = AllowedTransitions.TryGetValue(current, out var statuses) ? statuses : [];

            return allowed.Contains(next);
        }
    }
}
