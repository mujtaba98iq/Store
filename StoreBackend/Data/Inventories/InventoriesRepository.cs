using Domain.Inventories;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Inventories;

public class InventoriesRepository(ApplicationDbContext dbContext) : IInventoriesRepository
{
    public async Task<Inventory> Create(Inventory inventory)
    {
        dbContext.Inventories.Add(inventory);
        await dbContext.SaveChangesAsync();
        return inventory;
    }

    public async Task<List<Inventory>> FindByFilters(InventoryFilters inventoryFilters)
    {
        var query = dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, inventoryFilters);
        query = ApplyOrdering(query, inventoryFilters);
        query = ApplyPagination(query, inventoryFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Inventory> ApplyPagination(IQueryable<Inventory> query, InventoryFilters inventoryFilters)
    {
        var page = inventoryFilters.Page <= 0 ? 1 : inventoryFilters.Page;
        var pageSize = inventoryFilters.PageSize <= 0 ? 10 : inventoryFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Inventory> ApplyOrdering(IQueryable<Inventory> query, InventoryFilters inventoryFilters)
    {
        var inventoryOrderBy = inventoryFilters.OrderBy ?? InventoryOrderBy.CreatedAt;
        var orderDirection = inventoryFilters.OrderByDirection ?? OrderDirection.Desc;

        return inventoryOrderBy switch
        {
            InventoryOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.CreatedAt)
                : query.OrderByDescending(i => i.CreatedAt),
            InventoryOrderBy.Quantity => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.Quantity)
                : query.OrderByDescending(i => i.Quantity),
            InventoryOrderBy.ReservedQuantity => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.ReservedQuantity)
                : query.OrderByDescending(i => i.ReservedQuantity),
            // AvailableQuantity is not a column, so it is ordered by the expression behind it.
            InventoryOrderBy.AvailableQuantity => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.Quantity - i.ReservedQuantity)
                : query.OrderByDescending(i => i.Quantity - i.ReservedQuantity),
            InventoryOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.UpdatedAt)
                : query.OrderByDescending(i => i.UpdatedAt),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.CreatedAt)
                : query.OrderByDescending(i => i.CreatedAt)
        };
    }

    private static IQueryable<Inventory> ApplyFilters(IQueryable<Inventory> query, InventoryFilters inventoryFilters)
    {
        if (inventoryFilters.InventoryId != null)
        {
            query = query.Where(i => i.Id == inventoryFilters.InventoryId);
        }

        if (inventoryFilters.ProductVariantId != null)
        {
            query = query.Where(i => i.ProductVariantId == inventoryFilters.ProductVariantId);
        }

        if (inventoryFilters.MinQuantity.HasValue)
        {
            query = query.Where(i => i.Quantity >= inventoryFilters.MinQuantity.Value);
        }

        if (inventoryFilters.MaxQuantity.HasValue)
        {
            query = query.Where(i => i.Quantity <= inventoryFilters.MaxQuantity.Value);
        }

        if (inventoryFilters.IsAvailable.HasValue)
        {
            query = inventoryFilters.IsAvailable.Value
                ? query.Where(i => i.Quantity - i.ReservedQuantity > 0)
                : query.Where(i => i.Quantity - i.ReservedQuantity <= 0);
        }

        return query;
    }

    public async Task<Inventory?> FindById(Guid id)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);
        return inventory;
    }

    public async Task<Inventory?> FindByProductVariantId(Guid productVariantId)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId && i.DeletedAt == null);
        return inventory;
    }

    public async Task<Inventory> Update(Inventory inventory)
    {
        dbContext.Inventories.Update(inventory);
        await dbContext.SaveChangesAsync();
        return inventory;
    }

    public async Task<int> GetTotalCountByFilters(InventoryFilters inventoryFilters)
    {
        var query = dbContext.Inventories.AsNoTracking()
            .Where(i => i.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, inventoryFilters);
        return await query.CountAsync();
    }
}
