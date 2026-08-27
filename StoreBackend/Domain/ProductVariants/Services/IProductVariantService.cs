using Sheard.Type;

namespace Domain.ProductVariants;

public interface IProductVariantService
{
    Task<ProductVariant> Create(CreateProductVariantParams createProductVariantParams);
    Task<ProductVariant?> FindById(Guid id);
    Task<ProductVariant> Update(UpdateProductVariantParams updateProductVariantParams);
    Task<PaginationResult<ProductVariant>> Search(ProductVariantFilters productVariantFilters);
}
