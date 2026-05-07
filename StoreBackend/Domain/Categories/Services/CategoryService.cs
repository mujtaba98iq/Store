using Sheard.Type;

namespace Domain.Categories
{
    public class CategoryService(ICategoriesRepository categoriesRepository) : ICategoryService
    {
        public async Task<Category> Create(CreateCategoryParams createCategoryParams)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = createCategoryParams.Name,
                Description = createCategoryParams.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createCategoryParams.CreatedById
            };

            return await categoriesRepository.Create(category);
        }

        public async Task<Category?> FindById(Guid id)
        {
            return await categoriesRepository.FindById(id);
        }

        public async Task<PaginationResult<Category>> Search(CategoryFilters categoryFilters)
        {
            var categories = await categoriesRepository.FindByFilters(categoryFilters);
            var totalCount = await categoriesRepository.GetTotalCountByFilters(categoryFilters);

            return new PaginationResult<Category>
            {
                TotalCount = totalCount,
                Data = categories
            };
        }

        public async Task<Category> Update(UpdateCategoryParams updateCategoryParams)
        {
            var category = await categoriesRepository.FindById(updateCategoryParams.Id) 
                           ?? throw new ResourceNotFoundException("Category", $"Category with ID {updateCategoryParams.Id} not found");

            category.Name = updateCategoryParams.Name ?? category.Name;
            category.Description = updateCategoryParams.Description ?? category.Description;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedById = updateCategoryParams.UpdatedById;

            return await categoriesRepository.Update(category);
        } 
    }
}
