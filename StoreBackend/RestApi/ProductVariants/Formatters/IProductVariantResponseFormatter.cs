using Domain.ProductVariants;

namespace RestApi.ProductVariants;

public interface IProductVariantResponseFormatter
{
    ProductVariantResponse One(ProductVariant productVariant);
    ProductVariantListResponse Many(IEnumerable<ProductVariant> productVariants, int totalCount);
}
