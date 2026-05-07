using Domain.Categories;

namespace RestApi.Categories;

public class CategoryResponseFormatter : ICategoryResponseFormatter
{
    public CategoryListResponse Many(IEnumerable<Category> categories, int totalCount)
    {
        var categoryResults = categories.Select(One).ToList();

        return new CategoryListResponse
        {
            Data = categoryResults,
            TotalCount = totalCount
        };
    }

    public CategoryResponse One(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id.ToString(),
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            CreatedById = category.CreatedById,
            UpdatedById = category.UpdatedById
        };
    }
}
