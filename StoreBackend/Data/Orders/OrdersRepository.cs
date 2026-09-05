using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Orders;

public class OrdersRepository(ApplicationDbContext dbContext) : IOrdersRepository
{
    public async Task<Order> Create(Order order)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return order;
    }

    public async Task<List<Order>> FindByFilters(OrderFilters orderFilters)
    {
        var query = WithDetails(dbContext.Orders.AsNoTracking())
            .Where(o => o.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, orderFilters);
        query = ApplyOrdering(query, orderFilters);
        query = ApplyPagination(query, orderFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Order> ApplyPagination(IQueryable<Order> query, OrderFilters orderFilters)
    {
        var page = orderFilters.Page <= 0 ? 1 : orderFilters.Page;
        var pageSize = orderFilters.PageSize <= 0 ? 10 : orderFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Order> ApplyOrdering(IQueryable<Order> query, OrderFilters orderFilters)
    {
        var orderOrderBy = orderFilters.OrderBy ?? OrderOrderBy.CreatedAt;
        var orderDirection = orderFilters.OrderByDirection ?? OrderDirection.Desc;

        return orderOrderBy switch
        {
            OrderOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(o => o.CreatedAt)
                : query.OrderByDescending(o => o.CreatedAt),
            OrderOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(o => o.UpdatedAt)
                : query.OrderByDescending(o => o.UpdatedAt),
            OrderOrderBy.TotalAmount => orderDirection == OrderDirection.Asc
                ? query.OrderBy(o => o.TotalAmount)
                : query.OrderByDescending(o => o.TotalAmount),
            // Sorts by the lifecycle order the enum is numbered in, so Pending groups ahead
            // of Shipped rather than the two landing alphabetically.
            OrderOrderBy.Status => orderDirection == OrderDirection.Asc
                ? query.OrderBy(o => o.Status)
                : query.OrderByDescending(o => o.Status),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(o => o.CreatedAt)
                : query.OrderByDescending(o => o.CreatedAt)
        };
    }

    private static IQueryable<Order> ApplyFilters(IQueryable<Order> query, OrderFilters orderFilters)
    {
        if (orderFilters.OrderId != null)
        {
            query = query.Where(o => o.Id == orderFilters.OrderId);
        }

        if (orderFilters.UserId != null)
        {
            query = query.Where(o => o.UserId == orderFilters.UserId);
        }

        if (!string.IsNullOrWhiteSpace(orderFilters.OrderNumber))
        {
            query = query.Where(o => o.OrderNumber == orderFilters.OrderNumber);
        }

        if (orderFilters.Status.HasValue)
        {
            query = query.Where(o => o.Status == orderFilters.Status.Value);
        }

        if (orderFilters.CreatedFrom.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= orderFilters.CreatedFrom.Value);
        }

        if (orderFilters.CreatedTo.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= orderFilters.CreatedTo.Value);
        }

        if (orderFilters.MinTotalAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount >= orderFilters.MinTotalAmount.Value);
        }

        if (orderFilters.MaxTotalAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount <= orderFilters.MaxTotalAmount.Value);
        }

        return query;
    }

    public async Task<Order?> FindById(Guid id)
    {
        var order = await WithDetails(dbContext.Orders.AsNoTracking())
            .FirstOrDefaultAsync(o => o.Id == id && o.DeletedAt == null);
        return order;
    }

    public async Task<Order?> FindByOrderNumber(string orderNumber)
    {
        var order = await WithDetails(dbContext.Orders.AsNoTracking())
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.DeletedAt == null);
        return order;
    }

    public async Task<Order> Update(Order order)
    {
        // Written straight to the row rather than through the change tracker, for the same
        // reason as carts: the order carries the lines it was loaded with and saving it as a
        // graph would write those back too. Only the status and the audit columns move — what
        // was bought, and for how much, is frozen the moment the order is placed.
        await dbContext.Orders
            .Where(o => o.Id == order.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.Status, order.Status)
                .SetProperty(o => o.UpdatedAt, order.UpdatedAt)
                .SetProperty(o => o.UpdatedById, order.UpdatedById)
                .SetProperty(o => o.DeletedAt, order.DeletedAt)
                .SetProperty(o => o.DeletedById, order.DeletedById));

        return order;
    }

    public async Task<int> GetTotalCountByFilters(OrderFilters orderFilters)
    {
        var query = dbContext.Orders.AsNoTracking()
            .Where(o => o.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, orderFilters);
        return await query.CountAsync();
    }

    private static IQueryable<Order> WithDetails(IQueryable<Order> query)
    {
        // The address comes back with the order rather than on request: it is part of what
        // was agreed, the same as the lines, and a reader of one wants the other.
        return query
            .Include(o => o.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.CreatedAt))
            .Include(o => o.ShippingAddress);
    }
}
