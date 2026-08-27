using Domain.Exeptions;
using Domain.Products;
using Sheard.Type;

namespace Domain.ProductImages
{
    public class ProductImageService(IProductImagesRepository productImagesRepository, IProductsRepository productsRepository) : IProductImageService
    {
        public async Task<ProductImage> Create(CreateProductImageParams createProductImageParams)
        {
            _ = await productsRepository.FindById(createProductImageParams.ProductId)
                ?? throw new ResourceNotFoundException("Product", $"Product with ID {createProductImageParams.ProductId} not found");

            var isPrimary = createProductImageParams.IsPrimary ?? false;

            var productImage = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = createProductImageParams.ProductId,
                ImageUrl = createProductImageParams.ImageUrl,
                IsPrimary = isPrimary,
                DisplayOrder = createProductImageParams.DisplayOrder ?? 0,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createProductImageParams.CreatedById
            };

            if (isPrimary)
            {
                await DemoteCurrentPrimary(productImage.ProductId, productImage.Id, createProductImageParams.CreatedById);
            }

            return await productImagesRepository.Create(productImage);
        }

        public async Task<ProductImage?> FindById(Guid id)
        {
            return await productImagesRepository.FindById(id);
        }

        public async Task<PaginationResult<ProductImage>> Search(ProductImageFilters productImageFilters)
        {
            var productImages = await productImagesRepository.FindByFilters(productImageFilters);
            var totalCount = await productImagesRepository.GetTotalCountByFilters(productImageFilters);

            return new PaginationResult<ProductImage>
            {
                TotalCount = totalCount,
                Data = productImages
            };
        }

        public async Task<ProductImage> Update(UpdateProductImageParams updateProductImageParams)
        {
            var productImage = await productImagesRepository.FindById(updateProductImageParams.Id)
                               ?? throw new ResourceNotFoundException("ProductImage", $"Product image with ID {updateProductImageParams.Id} not found");

            if (updateProductImageParams.IsPrimary == true && !productImage.IsPrimary)
            {
                await DemoteCurrentPrimary(productImage.ProductId, productImage.Id, updateProductImageParams.UpdatedById);
            }

            productImage.ImageUrl = updateProductImageParams.ImageUrl ?? productImage.ImageUrl;
            productImage.IsPrimary = updateProductImageParams.IsPrimary ?? productImage.IsPrimary;
            productImage.DisplayOrder = updateProductImageParams.DisplayOrder ?? productImage.DisplayOrder;
            productImage.UpdatedAt = DateTime.UtcNow;
            productImage.UpdatedById = updateProductImageParams.UpdatedById;

            return await productImagesRepository.Update(productImage);
        }

        private async Task DemoteCurrentPrimary(Guid productId, Guid productImageId, string updatedById)
        {
            var currentPrimary = await productImagesRepository.FindPrimaryByProductId(productId);
            if (currentPrimary == null || currentPrimary.Id == productImageId)
            {
                return;
            }

            currentPrimary.IsPrimary = false;
            currentPrimary.UpdatedAt = DateTime.UtcNow;
            currentPrimary.UpdatedById = updatedById;

            await productImagesRepository.Update(currentPrimary);
        }
    }
}
