using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Carts;

public class CartsRepository(ApplicationDbContext dbContext) : ICartsRepository
{
    public async Task<Cart> Create(Cart cart)
    {
        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();
        return cart;
    }

    public async Task<List<Cart>> FindByFilters(CartFilters cartFilters)
    {
        var query = WithItems(dbContext.Carts.AsNoTracking())
            .Where(c => c.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, cartFilters);
        query = ApplyOrdering(query, cartFilters);
        query = ApplyPagination(query, cartFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Cart> ApplyPagination(IQueryable<Cart> query, CartFilters cartFilters)
    {
        var page = cartFilters.Page <= 0 ? 1 : cartFilters.Page;
        var pageSize = cartFilters.PageSize <= 0 ? 10 : cartFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Cart> ApplyOrdering(IQueryable<Cart> query, CartFilters cartFilters)
    {
        var cartOrderBy = cartFilters.OrderBy ?? CartOrderBy.CreatedAt;
        var orderDirection = cartFilters.OrderByDirection ?? OrderDirection.Desc;

        return cartOrderBy switch
        {
            CartOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt),
            CartOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.UpdatedAt)
                : query.OrderByDescending(c => c.UpdatedAt),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static IQueryable<Cart> ApplyFilters(IQueryable<Cart> query, CartFilters cartFilters)
    {
        if (cartFilters.CartId != null)
        {
            query = query.Where(c => c.Id == cartFilters.CartId);
        }

        if (cartFilters.UserId != null)
        {
            query = query.Where(c => c.UserId == cartFilters.UserId);
        }

        if (cartFilters.IsEmpty.HasValue)
        {
            query = cartFilters.IsEmpty.Value
                ? query.Where(c => !c.Items.Any(i => i.DeletedAt == null))
                : query.Where(c => c.Items.Any(i => i.DeletedAt == null));
        }

        return query;
    }

    public async Task<Cart?> FindById(Guid id)
    {
        var cart = await WithItems(dbContext.Carts.AsNoTracking())
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        return cart;
    }

    public async Task<Cart?> FindByUserId(Guid userId)
    {
        var cart = await WithItems(dbContext.Carts.AsNoTracking())
            .FirstOrDefaultAsync(c => c.UserId == userId && c.DeletedAt == null);
        return cart;
    }

    public async Task<Cart> Update(Cart cart)
    {
        // Written straight to the row instead of through the change tracker. The cart carries
        // the lines it was loaded with, and by the time it is saved those copies are stale:
        // they were changed through the cart item repository. Saving the cart as a graph would
        // write them back and undo that. Only the audit columns are set, because what a cart
        // belongs to and when it was opened never changes.
        await dbContext.Carts
            .Where(c => c.Id == cart.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.UpdatedAt, cart.UpdatedAt)
                .SetProperty(c => c.UpdatedById, cart.UpdatedById)
                .SetProperty(c => c.DeletedAt, cart.DeletedAt)
                .SetProperty(c => c.DeletedById, cart.DeletedById));

        return cart;
    }

    public async Task<int> GetTotalCountByFilters(CartFilters cartFilters)
    {
        var query = dbContext.Carts.AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, cartFilters);
        return await query.CountAsync();
    }

    /// <summary>
    /// Removed lines are kept in the table for audit, so they are filtered out here instead:
    /// a cart must never hand back something the customer already took out of it.
    /// </summary>
    private static IQueryable<Cart> WithItems(IQueryable<Cart> query)
    {
        return query.Include(c => c.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.CreatedAt));
    }
}
