namespace Domain.Categories;

public interface ICategoriesRepository
{
    Task<Category> Create(Category category);
    Task<Category> Update(Category category);
    Task<Category?> FindById(Guid id);
    Task<List<Category>> FindByFilters(CategoryFilters categoryFilters);
    Task<int> GetTotalCountByFilters(CategoryFilters categoryFilters);
    Task<List<Category>> FindByIds(List<Guid> ids);
}
