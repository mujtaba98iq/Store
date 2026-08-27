using Domain.Exeptions;
using Domain.Products;
using Sheard.Type;

namespace Domain.ProductVariants
{
    public class ProductVariantService(IProductVariantsRepository productVariantsRepository, IProductsRepository productsRepository) : IProductVariantService
    {
        public async Task<ProductVariant> Create(CreateProductVariantParams createProductVariantParams)
        {
            _ = await productsRepository.FindById(createProductVariantParams.ProductId)
                ?? throw new ResourceNotFoundException("Product", $"Product with ID {createProductVariantParams.ProductId} not found");

            await EnsureSkuIsAvailable(createProductVariantParams.Sku);

            var productVariant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = createProductVariantParams.ProductId,
                Sku = createProductVariantParams.Sku,
                Price = createProductVariantParams.Price,
                Barcode = createProductVariantParams.Barcode,
                IsActive = createProductVariantParams.IsActive ?? true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createProductVariantParams.CreatedById
            };

            return await productVariantsRepository.Create(productVariant);
        }

        public async Task<ProductVariant?> FindById(Guid id)
        {
            return await productVariantsRepository.FindById(id);
        }

        public async Task<PaginationResult<ProductVariant>> Search(ProductVariantFilters productVariantFilters)
        {
            var productVariants = await productVariantsRepository.FindByFilters(productVariantFilters);
            var totalCount = await productVariantsRepository.GetTotalCountByFilters(productVariantFilters);

            return new PaginationResult<ProductVariant>
            {
                TotalCount = totalCount,
                Data = productVariants
            };
        }

        public async Task<ProductVariant> Update(UpdateProductVariantParams updateProductVariantParams)
        {
            var productVariant = await productVariantsRepository.FindById(updateProductVariantParams.Id)
                                 ?? throw new ResourceNotFoundException("ProductVariant", $"Product variant with ID {updateProductVariantParams.Id} not found");

            if (updateProductVariantParams.Sku != null && updateProductVariantParams.Sku != productVariant.Sku)
            {
                await EnsureSkuIsAvailable(updateProductVariantParams.Sku);
                productVariant.Sku = updateProductVariantParams.Sku;
            }

            productVariant.Price = updateProductVariantParams.Price ?? productVariant.Price;
            productVariant.Barcode = updateProductVariantParams.Barcode ?? productVariant.Barcode;
            productVariant.IsActive = updateProductVariantParams.IsActive ?? productVariant.IsActive;
            productVariant.UpdatedAt = DateTime.UtcNow;
            productVariant.UpdatedById = updateProductVariantParams.UpdatedById;

            return await productVariantsRepository.Update(productVariant);
        }

        private async Task EnsureSkuIsAvailable(string sku)
        {
            var existing = await productVariantsRepository.FindBySku(sku);
            if (existing != null)
            {
                throw new ResourceAlreadyExistsException(sku);
            }
        }
    }
}
