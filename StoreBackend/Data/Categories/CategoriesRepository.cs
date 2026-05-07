using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Categories;

public class CategoriesRepository(ApplicationDbContext dbContext) : ICategoriesRepository
{
    public async Task<Category> Create(Category category)
    {
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        return category;
    }

    public async Task<List<Category>> FindByFilters(CategoryFilters categoryFilters)
    {
        var query = dbContext.Categories.AsNoTracking()
            .Where(g => g.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, categoryFilters);
        query = ApplyOrdering(query, categoryFilters);
        query = ApplyPagination(query, categoryFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Category> ApplyPagination(IQueryable<Category> query, CategoryFilters categoryFilters)
    {
        var page = categoryFilters.Page <= 0 ? 1 : categoryFilters.Page;
        var pageSize = categoryFilters.PageSize <= 0 ? 10 : categoryFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Category> ApplyOrdering(IQueryable<Category> query, CategoryFilters categoryFilters)
    {
        var categoryOrderBy = categoryFilters.OrderBy ?? CategoryOrderBy.CreatedAt;
        var orderDirection = categoryFilters.OrderByDirection ?? OrderDirection.Desc;

        return categoryOrderBy switch
        {
            CategoryOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(g => g.CreatedAt)
                : query.OrderByDescending(g => g.CreatedAt),
            CategoryOrderBy.Name => orderDirection == OrderDirection.Asc
                ? query.OrderBy(g => g.Name)
                : query.OrderByDescending(g => g.Name),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static IQueryable<Category> ApplyFilters(IQueryable<Category> query, CategoryFilters categoryFilters)
    {
        if (categoryFilters.CategoryId != null)
        {
            query = query.Where(c => c.Id == categoryFilters.CategoryId);
        }

        if (!string.IsNullOrEmpty(categoryFilters.Name))
        {
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{categoryFilters.Name.ToLower()}%"));
        }

        if (!string.IsNullOrEmpty(categoryFilters.Description))
        {
            query = query.Where(c => c.Description != null && EF.Functions.Like(c.Description.ToLower(), $"%{categoryFilters.Description.ToLower()}%"));
        }

        return query;
    }

    public async Task<Category?> FindById(Guid id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        return category;
    }

    public async Task<Category> Update(Category category)
    {
        dbContext.Categories.Update(category);
        await dbContext.SaveChangesAsync();
        return category;
    }

    public async Task<int> GetTotalCountByFilters(CategoryFilters categoryFilters)
    {
        var query = dbContext.Categories.AsNoTracking().AsQueryable();
        query = ApplyFilters(query, categoryFilters);
        return await query.CountAsync();
    }

    public async Task<List<Category>> FindByIds(List<Guid> ids)
    {
        return await dbContext.Categories.Where(c => ids.Contains(c.Id)).ToListAsync();
    }
}
