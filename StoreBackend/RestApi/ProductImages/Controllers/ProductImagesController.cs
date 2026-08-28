using Domain.ProductImages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using RestApi.Validation;

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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(FormFileExtensions.MaxImageRequestSizeInBytes)]
        [ProducesResponseType(typeof(ProductImageResponse), 201)]
        [UseFormValidator(Validator = typeof(CreateProductImageRequestValidator))]
        public async Task<IActionResult> Create([FromForm] CreateProductImageRequest createProductImageRequest)
        {
            await using var imageContent = createProductImageRequest.Image.OpenReadStream();

            CreateProductImageParams createProductImageParams = new()
            {
                ProductId = createProductImageRequest.ProductId,
                ImageContent = imageContent,
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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(FormFileExtensions.MaxImageRequestSizeInBytes)]
        [ProducesResponseType(typeof(ProductImageResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseFormValidator(Validator = typeof(UpdateProductImageRequestValidator))]
        public async Task<IActionResult> Update(Guid productImageId, [FromForm] UpdateProductImageRequest request)
        {
            await using var imageContent = request.Image?.OpenReadStream();

            var updatedProductImage = await productImageService.Update(new UpdateProductImageParams
            {
                Id = productImageId,
                ImageContent = imageContent,
                IsPrimary = request.IsPrimary,
                DisplayOrder = request.DisplayOrder,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedProductImage));
        }

    }
}
