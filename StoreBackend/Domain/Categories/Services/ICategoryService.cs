using Sheard.Type;

namespace Domain.Categories;

public interface ICategoryService
{
    Task<Category> Create(CreateCategoryParams createCategoryParams);
    Task<Category?> FindById(Guid id);
    Task<Category> Update(UpdateCategoryParams updateCategoryParams);
    Task<PaginationResult<Category>> Search(CategoryFilters categoryFilters);

    /// <summary>Soft-deletes a category. False when it does not exist, or was already deleted.</summary>
    Task<bool> Delete(DeleteCategoryParams deleteCategoryParams);
}
