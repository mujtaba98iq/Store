using Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Shipments;

public class ShipmentsRepository(ApplicationDbContext dbContext) : IShipmentsRepository
{
    public async Task<Shipment> Create(Shipment shipment)
    {
        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync();
        return shipment;
    }

    public async Task<List<Shipment>> FindByFilters(ShipmentFilters shipmentFilters)
    {
        var query = WithOrder(dbContext.Shipments.AsNoTracking())
            .Where(s => s.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, shipmentFilters);
        query = ApplyOrdering(query, shipmentFilters);
        query = ApplyPagination(query, shipmentFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Shipment> ApplyPagination(IQueryable<Shipment> query, ShipmentFilters shipmentFilters)
    {
        var page = shipmentFilters.Page <= 0 ? 1 : shipmentFilters.Page;
        var pageSize = shipmentFilters.PageSize <= 0 ? 10 : shipmentFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Shipment> ApplyOrdering(IQueryable<Shipment> query, ShipmentFilters shipmentFilters)
    {
        var shipmentOrderBy = shipmentFilters.OrderBy ?? ShipmentOrderBy.CreatedAt;
        var orderDirection = shipmentFilters.OrderByDirection ?? OrderDirection.Desc;

        return shipmentOrderBy switch
        {
            ShipmentOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt),
            ShipmentOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(s => s.UpdatedAt)
                : query.OrderByDescending(s => s.UpdatedAt),
            ShipmentOrderBy.ShippedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(s => s.ShippedAt)
                : query.OrderByDescending(s => s.ShippedAt),
            ShipmentOrderBy.DeliveredAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(s => s.DeliveredAt)
                : query.OrderByDescending(s => s.DeliveredAt),
            // Sorts by the lifecycle order the enum is numbered in, so parcels still waiting
            // to go out group ahead of the ones already delivered.
            ShipmentOrderBy.Status => orderDirection == OrderDirection.Asc
                ? query.OrderBy(s => s.Status)
                : query.OrderByDescending(s => s.Status),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt)
        };
    }

    private static IQueryable<Shipment> ApplyFilters(IQueryable<Shipment> query, ShipmentFilters shipmentFilters)
    {
        if (shipmentFilters.ShipmentId != null)
        {
            query = query.Where(s => s.Id == shipmentFilters.ShipmentId);
        }

        if (shipmentFilters.OrderId != null)
        {
            query = query.Where(s => s.OrderId == shipmentFilters.OrderId);
        }

        // Reached through the order, a parcel having no customer of its own.
        if (shipmentFilters.UserId != null)
        {
            query = query.Where(s => s.Order != null && s.Order.UserId == shipmentFilters.UserId);
        }

        if (!string.IsNullOrWhiteSpace(shipmentFilters.TrackingNumber))
        {
            query = query.Where(s => s.TrackingNumber == shipmentFilters.TrackingNumber);
        }

        if (!string.IsNullOrWhiteSpace(shipmentFilters.ShippingProvider))
        {
            query = query.Where(s => s.ShippingProvider == shipmentFilters.ShippingProvider);
        }

        if (shipmentFilters.Status.HasValue)
        {
            query = query.Where(s => s.Status == shipmentFilters.Status.Value);
        }

        if (shipmentFilters.CreatedFrom.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= shipmentFilters.CreatedFrom.Value);
        }

        if (shipmentFilters.CreatedTo.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= shipmentFilters.CreatedTo.Value);
        }

        return query;
    }

    public async Task<Shipment?> FindById(Guid id)
    {
        var shipment = await WithOrder(dbContext.Shipments.AsNoTracking())
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);
        return shipment;
    }

    public async Task<Shipment?> FindByOrderId(Guid orderId)
    {
        var shipment = await WithOrder(dbContext.Shipments.AsNoTracking())
            .FirstOrDefaultAsync(s => s.OrderId == orderId && s.DeletedAt == null);
        return shipment;
    }

    public async Task<Shipment> Update(Shipment shipment)
    {
        // Written straight to the row rather than through the change tracker, as orders and
        // payments are: the shipment carries the order it was loaded with, and saving it as
        // a graph would write that back too. OrderId is left out — a parcel does not move
        // between orders.
        await dbContext.Shipments
            .Where(s => s.Id == shipment.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.TrackingNumber, shipment.TrackingNumber)
                .SetProperty(s => s.ShippingProvider, shipment.ShippingProvider)
                .SetProperty(s => s.Status, shipment.Status)
                .SetProperty(s => s.ShippedAt, shipment.ShippedAt)
                .SetProperty(s => s.DeliveredAt, shipment.DeliveredAt)
                .SetProperty(s => s.UpdatedAt, shipment.UpdatedAt)
                .SetProperty(s => s.UpdatedById, shipment.UpdatedById)
                .SetProperty(s => s.DeletedAt, shipment.DeletedAt)
                .SetProperty(s => s.DeletedById, shipment.DeletedById));

        return shipment;
    }

    public async Task<int> GetTotalCountByFilters(ShipmentFilters shipmentFilters)
    {
        var query = dbContext.Shipments.AsNoTracking()
            .Where(s => s.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, shipmentFilters);
        return await query.CountAsync();
    }

    private static IQueryable<Shipment> WithOrder(IQueryable<Shipment> query)
    {
        // The order comes back with the parcel because every reader needs it: the customer it
        // belongs to is the only thing that says who may look at the parcel at all.
        return query.Include(s => s.Order);
    }
}
