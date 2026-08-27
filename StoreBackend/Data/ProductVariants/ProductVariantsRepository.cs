using Domain.ProductVariants;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.ProductVariants;

public class ProductVariantsRepository(ApplicationDbContext dbContext) : IProductVariantsRepository
{
    public async Task<ProductVariant> Create(ProductVariant productVariant)
    {
        dbContext.ProductVariants.Add(productVariant);
        await dbContext.SaveChangesAsync();
        return productVariant;
    }

    public async Task<List<ProductVariant>> FindByFilters(ProductVariantFilters productVariantFilters)
    {
        var query = dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => v.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, productVariantFilters);
        query = ApplyOrdering(query, productVariantFilters);
        query = ApplyPagination(query, productVariantFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<ProductVariant> ApplyPagination(IQueryable<ProductVariant> query, ProductVariantFilters productVariantFilters)
    {
        var page = productVariantFilters.Page <= 0 ? 1 : productVariantFilters.Page;
        var pageSize = productVariantFilters.PageSize <= 0 ? 10 : productVariantFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<ProductVariant> ApplyOrdering(IQueryable<ProductVariant> query, ProductVariantFilters productVariantFilters)
    {
        var productVariantOrderBy = productVariantFilters.OrderBy ?? ProductVariantOrderBy.CreatedAt;
        var orderDirection = productVariantFilters.OrderByDirection ?? OrderDirection.Desc;

        return productVariantOrderBy switch
        {
            ProductVariantOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(v => v.CreatedAt)
                : query.OrderByDescending(v => v.CreatedAt),
            ProductVariantOrderBy.Sku => orderDirection == OrderDirection.Asc
                ? query.OrderBy(v => v.Sku)
                : query.OrderByDescending(v => v.Sku),
            ProductVariantOrderBy.Price => orderDirection == OrderDirection.Asc
                ? query.OrderBy(v => v.Price)
                : query.OrderByDescending(v => v.Price),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(v => v.CreatedAt)
                : query.OrderByDescending(v => v.CreatedAt)
        };
    }

    private static IQueryable<ProductVariant> ApplyFilters(IQueryable<ProductVariant> query, ProductVariantFilters productVariantFilters)
    {
        if (productVariantFilters.ProductVariantId != null)
        {
            query = query.Where(v => v.Id == productVariantFilters.ProductVariantId);
        }

        if (productVariantFilters.ProductId != null)
        {
            query = query.Where(v => v.ProductId == productVariantFilters.ProductId);
        }

        if (!string.IsNullOrEmpty(productVariantFilters.Sku))
        {
            query = query.Where(v => EF.Functions.Like(v.Sku.ToLower(), $"%{productVariantFilters.Sku.ToLower()}%"));
        }

        if (!string.IsNullOrEmpty(productVariantFilters.Barcode))
        {
            query = query.Where(v => v.Barcode != null && EF.Functions.Like(v.Barcode.ToLower(), $"%{productVariantFilters.Barcode.ToLower()}%"));
        }

        if (productVariantFilters.Price.HasValue)
        {
            query = query.Where(v => v.Price == productVariantFilters.Price.Value);
        }

        if (productVariantFilters.IsActive.HasValue)
        {
            query = query.Where(v => v.IsActive == productVariantFilters.IsActive.Value);
        }

        return query;
    }

    public async Task<ProductVariant?> FindById(Guid id)
    {
        var productVariant = await dbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null);
        return productVariant;
    }

    public async Task<ProductVariant?> FindBySku(string sku)
    {
        var productVariant = await dbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Sku == sku && v.DeletedAt == null);
        return productVariant;
    }

    public async Task<ProductVariant> Update(ProductVariant productVariant)
    {
        dbContext.ProductVariants.Update(productVariant);
        await dbContext.SaveChangesAsync();
        return productVariant;
    }

    public async Task<int> GetTotalCountByFilters(ProductVariantFilters productVariantFilters)
    {
        var query = dbContext.ProductVariants.AsNoTracking()
            .Where(v => v.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, productVariantFilters);
        return await query.CountAsync();
    }
}
