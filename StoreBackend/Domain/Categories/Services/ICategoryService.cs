using Sheard.Type;

namespace Domain.Categories;

public interface ICategoryService
{
    Task<Category> Create(CreateCategoryParams createCategoryParams);
    Task<Category?> FindById(Guid id);
    Task<Category> Update(UpdateCategoryParams updateCategoryParams);
    Task<PaginationResult<Category>> Search(CategoryFilters categoryFilters);
}
