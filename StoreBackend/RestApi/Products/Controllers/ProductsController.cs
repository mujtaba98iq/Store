using Domain.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Products.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService productService, IProductResponseFormatter responseFormatter) : ControllerBase
    {
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll(ProductFilters productFilters)
        {
            var products = productService.Search(productFilters);
            return Ok(await products);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateProductRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest createProductRequest)
        {
            CreateProductParams createProductParams = new()
            {
                Name = createProductRequest.Name,
                Description = createProductRequest.Description,
                Price = createProductRequest.Price,
                Quantity = createProductRequest.Quantity,
                ImagePath = createProductRequest.ImagePath,
                CreatedById = this.GetUserId()
            };

            var product = await productService.Create(createProductParams);
            var result = responseFormatter.One(product);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await productService.FindById(id);
            return product is null
                ? NotFound()
                : Ok(responseFormatter.One(product));
        }


        [HttpPatch("{productId}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateProductRequestValidator))]
        public async Task<IActionResult> Update(Guid productId, [FromBody] UpdateProductRequest request)
        {
            var updatedPolicyVersion = await productService.Update(new UpdateProductParams
            {
                Id = productId,    
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Quantity = request.Quantity,
                ImagePath = request.ImagePath,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedPolicyVersion));
        }

    }
}
