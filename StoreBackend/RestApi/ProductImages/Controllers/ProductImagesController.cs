using Domain.ProductImages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.ProductImages.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImagesController(IProductImageService productImageService, IProductImageResponseFormatter responseFormatter) : ControllerBase
    {
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductImageFilters productImageFilters)
        {
            var productImages = await productImageService.Search(productImageFilters);
            return Ok(responseFormatter.Many(productImages.Data, productImages.TotalCount));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductImageResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateProductImageRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateProductImageRequest createProductImageRequest)
        {
            CreateProductImageParams createProductImageParams = new()
            {
                ProductId = createProductImageRequest.ProductId,
                ImageUrl = createProductImageRequest.ImageUrl,
                IsPrimary = createProductImageRequest.IsPrimary,
                DisplayOrder = createProductImageRequest.DisplayOrder,
                CreatedById = this.GetUserId()
            };

            var productImage = await productImageService.Create(createProductImageParams);
            var result = responseFormatter.One(productImage);

            return CreatedAtAction(nameof(GetById), new { id = productImage.Id }, result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductImageResponse), 200)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var productImage = await productImageService.FindById(id);
            return productImage is null
                ? NotFound()
                : Ok(responseFormatter.One(productImage));
        }


        [HttpPatch("{productImageId}")]
        [ProducesResponseType(typeof(ProductImageResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateProductImageRequestValidator))]
        public async Task<IActionResult> Update(Guid productImageId, [FromBody] UpdateProductImageRequest request)
        {
            var updatedProductImage = await productImageService.Update(new UpdateProductImageParams
            {
                Id = productImageId,
                ImageUrl = request.ImageUrl,
                IsPrimary = request.IsPrimary,
                DisplayOrder = request.DisplayOrder,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedProductImage));
        }

    }
}
