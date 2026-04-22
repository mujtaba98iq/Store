using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Products;

public class ProductsRepository(ApplicationDbContext dbContext) : IProductsRepository
{
    
    public async Task<Product> Create(Product Product)
    {
        dbContext.Products.Add(Product);
        await dbContext.SaveChangesAsync();
        return Product;
    }

    public async Task<List<Product>> FindByFilters(ProductFilters productFilters)
    {
        var query = dbContext.Products.AsNoTracking()
            .Where(g => g.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, productFilters);
        query = ApplyOrdering(query, productFilters);
        query = ApplyPagination(query, productFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Product> ApplyPagination(IQueryable<Product> query, ProductFilters productFilters)
    {
        var page = productFilters.Page <= 0 ? 1 : productFilters.Page;
        var pageSize = productFilters.PageSize <= 0 ? 10 : productFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Product> ApplyOrdering(IQueryable<Product> query, ProductFilters productFilters)
    {
        var productOrderBy = productFilters.OrderBy ?? ProductOrderBy.CreatedAt;
        var orderDirection = productFilters.OrderByDirection ?? OrderDirection.Desc;

        return productOrderBy switch
        {
            ProductOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(g => g.CreatedAt)
                : query.OrderByDescending(g => g.CreatedAt),
            ProductOrderBy.Name => orderDirection == OrderDirection.Asc
                ? query.OrderBy(g => g.Name)
                : query.OrderByDescending(g => g.Name),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilters policyFilters)
    {
        if (policyFilters.ProductId != null)
        {
            query = query.Where(p => p.Id == policyFilters.ProductId);
        }

        if (!string.IsNullOrEmpty(policyFilters.Name))
        {
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{policyFilters.Name.ToLower()}%"));
        }

        if (!string.IsNullOrEmpty(policyFilters.Description))
        {
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{policyFilters.Description.ToLower()}%"));
        }

        if (policyFilters.Price.HasValue)
        {
            query = query.Where(c => c.Price == policyFilters.Price.Value);
        }

        if (policyFilters.Quantity.HasValue)
        {
            query = query.Where(c => c.Quantity == policyFilters.Quantity.Value);
        }

        return query;
    }

    public async Task<Product?> FindById(Guid id)
    {
        var product = await dbContext.Products.FindAsync(id);
        return product;
    }

    public async Task<Product> Update(Product Product)
    {
        dbContext.Products.Update(Product);
        await dbContext.SaveChangesAsync();
        return Product;
    }

    public async Task<int> GetTotalCountByFilters(ProductFilters productFilters)
    {
        var query = dbContext.Products.AsNoTracking().AsQueryable();
        query = ApplyFilters(query, productFilters);
        return await query.CountAsync();
    }
}
