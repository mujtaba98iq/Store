using Domain.ProductImages;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.ProductImages;

public class ProductImagesRepository(ApplicationDbContext dbContext) : IProductImagesRepository
{
    public async Task<ProductImage> Create(ProductImage productImage)
    {
        dbContext.ProductImages.Add(productImage);
        await dbContext.SaveChangesAsync();
        return productImage;
    }

    public async Task<List<ProductImage>> FindByFilters(ProductImageFilters productImageFilters)
    {
        var query = dbContext.ProductImages
            .AsNoTracking()
            .Where(i => i.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, productImageFilters);
        query = ApplyOrdering(query, productImageFilters);
        query = ApplyPagination(query, productImageFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<ProductImage> ApplyPagination(IQueryable<ProductImage> query, ProductImageFilters productImageFilters)
    {
        var page = productImageFilters.Page <= 0 ? 1 : productImageFilters.Page;
        var pageSize = productImageFilters.PageSize <= 0 ? 10 : productImageFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<ProductImage> ApplyOrdering(IQueryable<ProductImage> query, ProductImageFilters productImageFilters)
    {
        var productImageOrderBy = productImageFilters.OrderBy ?? ProductImageOrderBy.DisplayOrder;
        var orderDirection = productImageFilters.OrderByDirection ?? OrderDirection.Desc;

        return productImageOrderBy switch
        {
            ProductImageOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.CreatedAt)
                : query.OrderByDescending(i => i.CreatedAt),
            ProductImageOrderBy.DisplayOrder => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.DisplayOrder)
                : query.OrderByDescending(i => i.DisplayOrder),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(i => i.CreatedAt)
                : query.OrderByDescending(i => i.CreatedAt)
        };
    }

    private static IQueryable<ProductImage> ApplyFilters(IQueryable<ProductImage> query, ProductImageFilters productImageFilters)
    {
        if (productImageFilters.ProductImageId != null)
        {
            query = query.Where(i => i.Id == productImageFilters.ProductImageId);
        }

        if (productImageFilters.ProductId != null)
        {
            query = query.Where(i => i.ProductId == productImageFilters.ProductId);
        }

        if (!string.IsNullOrEmpty(productImageFilters.ImageUrl))
        {
            query = query.Where(i => EF.Functions.Like(i.ImageUrl.ToLower(), $"%{productImageFilters.ImageUrl.ToLower()}%"));
        }

        if (productImageFilters.IsPrimary.HasValue)
        {
            query = query.Where(i => i.IsPrimary == productImageFilters.IsPrimary.Value);
        }

        if (productImageFilters.DisplayOrder.HasValue)
        {
            query = query.Where(i => i.DisplayOrder == productImageFilters.DisplayOrder.Value);
        }

        return query;
    }

    public async Task<ProductImage?> FindById(Guid id)
    {
        var productImage = await dbContext.ProductImages
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);
        return productImage;
    }

    public async Task<ProductImage?> FindPrimaryByProductId(Guid productId)
    {
        var productImage = await dbContext.ProductImages
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.IsPrimary && i.DeletedAt == null);
        return productImage;
    }

    public async Task<ProductImage> Update(ProductImage productImage)
    {
        dbContext.ProductImages.Update(productImage);
        await dbContext.SaveChangesAsync();
        return productImage;
    }

    public async Task<int> GetTotalCountByFilters(ProductImageFilters productImageFilters)
    {
        var query = dbContext.ProductImages.AsNoTracking()
            .Where(i => i.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, productImageFilters);
        return await query.CountAsync();
    }
}
