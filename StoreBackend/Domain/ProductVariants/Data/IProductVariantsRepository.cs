namespace Domain.ProductVariants;

public interface IProductVariantsRepository
{
    Task<ProductVariant> Create(ProductVariant productVariant);
    Task<ProductVariant> Update(ProductVariant productVariant);
    Task<ProductVariant?> FindById(Guid id);
    Task<ProductVariant?> FindBySku(string sku);
    Task<List<ProductVariant>> FindByFilters(ProductVariantFilters productVariantFilters);
    Task<int> GetTotalCountByFilters(ProductVariantFilters productVariantFilters);
}
