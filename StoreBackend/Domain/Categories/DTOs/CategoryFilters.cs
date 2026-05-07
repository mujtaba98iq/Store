using Sheard.Type;

namespace Domain.Categories;

public class CategoryFilters : ListingOptions
{
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public CategoryOrderBy? OrderBy { get; set; } = CategoryOrderBy.CreatedAt;
}
