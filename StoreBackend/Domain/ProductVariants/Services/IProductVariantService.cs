using Sheard.Type;

namespace Domain.ProductVariants;

public interface IProductVariantService
{
    Task<ProductVariant> Create(CreateProductVariantParams createProductVariantParams);
    Task<ProductVariant?> FindById(Guid id);
    Task<ProductVariant> Update(UpdateProductVariantParams updateProductVariantParams);
    Task<PaginationResult<ProductVariant>> Search(ProductVariantFilters productVariantFilters);

    /// <summary>Soft-deletes a variant. False when it does not exist, or was already deleted.</summary>
    Task<bool> Delete(DeleteProductVariantParams deleteProductVariantParams);
}
