using Domain.Categories;

namespace RestApi.Categories;

public interface ICategoryResponseFormatter
{
    CategoryResponse One(Category category);
    CategoryListResponse Many(IEnumerable<Category> categories, int totalCount);
}
