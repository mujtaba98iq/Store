using Domain.Wishlists;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Wishlists;

public class WishlistItemsRepository(ApplicationDbContext dbContext) : IWishlistItemsRepository
{
    public async Task<WishlistItem> Create(WishlistItem wishlistItem)
    {
        dbContext.WishlistItems.Add(wishlistItem);
        await dbContext.SaveChangesAsync();
        return wishlistItem;
    }

    public async Task<List<WishlistItem>> FindByFilters(WishlistItemFilters wishlistItemFilters)
    {
        var query = dbContext.WishlistItems.AsNoTracking()
            .Where(w => w.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, wishlistItemFilters);
        query = ApplyOrdering(query, wishlistItemFilters);
        query = ApplyPagination(query, wishlistItemFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<WishlistItem> ApplyPagination(IQueryable<WishlistItem> query, WishlistItemFilters wishlistItemFilters)
    {
        var page = wishlistItemFilters.Page <= 0 ? 1 : wishlistItemFilters.Page;
        var pageSize = wishlistItemFilters.PageSize <= 0 ? 10 : wishlistItemFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<WishlistItem> ApplyOrdering(IQueryable<WishlistItem> query, WishlistItemFilters wishlistItemFilters)
    {
        var orderDirection = wishlistItemFilters.OrderByDirection ?? OrderDirection.Desc;

        // When it was added is all there is to order by, so the direction is the only
        // choice the caller gets. Descending by default: the newest thing a customer
        // wanted is the one they are looking for.
        return orderDirection == OrderDirection.Asc
            ? query.OrderBy(w => w.CreatedAt)
            : query.OrderByDescending(w => w.CreatedAt);
    }

    private static IQueryable<WishlistItem> ApplyFilters(IQueryable<WishlistItem> query, WishlistItemFilters wishlistItemFilters)
    {
        if (wishlistItemFilters.WishlistItemId != null)
        {
            query = query.Where(w => w.Id == wishlistItemFilters.WishlistItemId);
        }

        if (wishlistItemFilters.UserId != null)
        {
            query = query.Where(w => w.UserId == wishlistItemFilters.UserId);
        }

        if (wishlistItemFilters.ProductId != null)
        {
            query = query.Where(w => w.ProductId == wishlistItemFilters.ProductId);
        }

        if (wishlistItemFilters.CreatedFrom.HasValue)
        {
            query = query.Where(w => w.CreatedAt >= wishlistItemFilters.CreatedFrom.Value);
        }

        if (wishlistItemFilters.CreatedTo.HasValue)
        {
            query = query.Where(w => w.CreatedAt <= wishlistItemFilters.CreatedTo.Value);
        }

        return query;
    }

    public async Task<WishlistItem?> FindById(Guid id)
    {
        var wishlistItem = await dbContext.WishlistItems
            .FirstOrDefaultAsync(w => w.Id == id && w.DeletedAt == null);
        return wishlistItem;
    }

    public async Task<WishlistItem?> FindByUserAndProduct(Guid userId, Guid productId)
    {
        var wishlistItem = await dbContext.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId
                                      && w.ProductId == productId
                                      && w.DeletedAt == null);
        return wishlistItem;
    }

    public async Task<List<WishlistItem>> FindByUserId(Guid userId)
    {
        return await dbContext.WishlistItems
            .Where(w => w.UserId == userId && w.DeletedAt == null)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<WishlistItem> Update(WishlistItem wishlistItem)
    {
        dbContext.WishlistItems.Update(wishlistItem);
        await dbContext.SaveChangesAsync();
        return wishlistItem;
    }

    public async Task<List<WishlistItem>> UpdateMany(List<WishlistItem> wishlistItems)
    {
        dbContext.WishlistItems.UpdateRange(wishlistItems);
        await dbContext.SaveChangesAsync();
        return wishlistItems;
    }

    public async Task<int> GetTotalCountByFilters(WishlistItemFilters wishlistItemFilters)
    {
        var query = dbContext.WishlistItems.AsNoTracking()
            .Where(w => w.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, wishlistItemFilters);
        return await query.CountAsync();
    }
}
