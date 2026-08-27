using Domain.ProductVariants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.ProductVariants.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantsController(IProductVariantService productVariantService, IProductVariantResponseFormatter responseFormatter) : ControllerBase
    {
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductVariantFilters productVariantFilters)
        {
            var productVariants = await productVariantService.Search(productVariantFilters);
            return Ok(responseFormatter.Many(productVariants.Data, productVariants.TotalCount));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductVariantResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateProductVariantRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantRequest createProductVariantRequest)
        {
            CreateProductVariantParams createProductVariantParams = new()
            {
                ProductId = createProductVariantRequest.ProductId,
                Sku = createProductVariantRequest.Sku,
                Price = createProductVariantRequest.Price,
                Barcode = createProductVariantRequest.Barcode,
                IsActive = createProductVariantRequest.IsActive,
                CreatedById = this.GetUserId()
            };

            var productVariant = await productVariantService.Create(createProductVariantParams);
            var result = responseFormatter.One(productVariant);

            return CreatedAtAction(nameof(GetById), new { id = productVariant.Id }, result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductVariantResponse), 200)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var productVariant = await productVariantService.FindById(id);
            return productVariant is null
                ? NotFound()
                : Ok(responseFormatter.One(productVariant));
        }


        [HttpPatch("{productVariantId}")]
        [ProducesResponseType(typeof(ProductVariantResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateProductVariantRequestValidator))]
        public async Task<IActionResult> Update(Guid productVariantId, [FromBody] UpdateProductVariantRequest request)
        {
            var updatedProductVariant = await productVariantService.Update(new UpdateProductVariantParams
            {
                Id = productVariantId,
                Sku = request.Sku,
                Price = request.Price,
                Barcode = request.Barcode,
                IsActive = request.IsActive,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedProductVariant));
        }

    }
}
