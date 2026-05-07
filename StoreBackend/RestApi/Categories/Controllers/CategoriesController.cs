using Domain.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Categories.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(ICategoryService categoryService, ICategoryResponseFormatter responseFormatter) : ControllerBase
    {
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CategoryFilters categoryFilters)
        {
            var categories = await categoryService.Search(categoryFilters);
            return Ok(responseFormatter.Many(categories.Data, categories.TotalCount));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateCategoryRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            CreateCategoryParams createCategoryParams = new()
            {
                Name = createCategoryRequest.Name,
                Description = createCategoryRequest.Description,
                CreatedById = this.GetUserId()
            };

            var category = await categoryService.Create(createCategoryParams);
            var result = responseFormatter.One(category);

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryResponse), 200)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await categoryService.FindById(id);
            return category is null
                ? NotFound()
                : Ok(responseFormatter.One(category));
        }


        [HttpPatch("{categoryId}")]
        [ProducesResponseType(typeof(CategoryResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateCategoryRequestValidator))]
        public async Task<IActionResult> Update(Guid categoryId, [FromBody] UpdateCategoryRequest request)
        {
            var updatedCategory = await categoryService.Update(new UpdateCategoryParams
            {
                Id = categoryId,    
                Name = request.Name,
                Description = request.Description,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedCategory));
        }

    }
}
