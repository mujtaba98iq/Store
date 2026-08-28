using Domain.Exeptions;
using Domain.Products;
using Domain.Storage;
using Microsoft.Extensions.Logging;
using Sheard.Type;

namespace Domain.ProductImages
{
    public class ProductImageService(
        IProductImagesRepository productImagesRepository,
        IProductsRepository productsRepository,
        IImageStorageService imageStorageService,
        ILogger<ProductImageService> logger) : IProductImageService
    {
        private const string ImageFolder = "product-images";

        public async Task<ProductImage> Create(CreateProductImageParams createProductImageParams)
        {
            _ = await productsRepository.FindById(createProductImageParams.ProductId)
                ?? throw new ResourceNotFoundException("Product", $"Product with ID {createProductImageParams.ProductId} not found");

            var isPrimary = createProductImageParams.IsPrimary ?? false;

            // Upload first: a failed upload must never leave a ProductImage row behind.
            var storedImage = await imageStorageService.Upload(new UploadImageParams
            {
                Content = createProductImageParams.ImageContent,
                Folder = BuildFolder(createProductImageParams.ProductId)
            });

            var productImage = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = createProductImageParams.ProductId,
                ImageUrl = storedImage.ImageUrl,
                PublicId = storedImage.PublicId,
                IsPrimary = isPrimary,
                DisplayOrder = createProductImageParams.DisplayOrder ?? 0,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createProductImageParams.CreatedById
            };

            if (isPrimary)
            {
                await DemoteCurrentPrimary(productImage.ProductId, productImage.Id, createProductImageParams.CreatedById);
            }

            try
            {
                return await productImagesRepository.Create(productImage);
            }
            catch
            {
                // The row was not written, so the uploaded asset would be orphaned.
                await DeleteStoredImage(storedImage.PublicId);
                throw;
            }
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

            var replacedPublicId = productImage.PublicId;
            ImageStorageResult? storedImage = null;

            if (updateProductImageParams.ImageContent is not null)
            {
                storedImage = await imageStorageService.Upload(new UploadImageParams
                {
                    Content = updateProductImageParams.ImageContent,
                    Folder = BuildFolder(productImage.ProductId)
                });

                productImage.ImageUrl = storedImage.ImageUrl;
                productImage.PublicId = storedImage.PublicId;
            }

            productImage.IsPrimary = updateProductImageParams.IsPrimary ?? productImage.IsPrimary;
            productImage.DisplayOrder = updateProductImageParams.DisplayOrder ?? productImage.DisplayOrder;
            productImage.UpdatedAt = DateTime.UtcNow;
            productImage.UpdatedById = updateProductImageParams.UpdatedById;

            ProductImage updatedProductImage;

            try
            {
                updatedProductImage = await productImagesRepository.Update(productImage);
            }
            catch when (storedImage is not null)
            {
                // The row was not written, so the newly uploaded asset would be orphaned.
                await DeleteStoredImage(storedImage.PublicId);
                throw;
            }

            if (storedImage is not null && !string.IsNullOrWhiteSpace(replacedPublicId))
            {
                await DeleteStoredImage(replacedPublicId);
            }

            return updatedProductImage;
        }

        private static string BuildFolder(Guid productId)
        {
            return $"{ImageFolder}/{productId}";
        }

        private async Task DeleteStoredImage(string publicId)
        {
            // Never let clean up of a stored asset mask the result of the request itself.
            try
            {
                await imageStorageService.Delete(publicId);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not delete stored image {PublicId}", publicId);
            }
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
