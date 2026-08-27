using Domain.ProductVariants;

namespace RestApi.ProductVariants;

public class ProductVariantResponseFormatter : IProductVariantResponseFormatter
{
    public ProductVariantListResponse Many(IEnumerable<ProductVariant> productVariants, int totalCount)
    {
        var productVariantResults = productVariants.Select(One).ToList();

        return new ProductVariantListResponse
        {
            Data = productVariantResults,
            TotalCount = totalCount
        };
    }

    public ProductVariantResponse One(ProductVariant productVariant)
    {
        return new ProductVariantResponse
        {
            Id = productVariant.Id.ToString(),
            ProductId = productVariant.ProductId.ToString(),
            Sku = productVariant.Sku,
            Price = productVariant.Price,
            Barcode = productVariant.Barcode,
            IsActive = productVariant.IsActive,
            CreatedAt = productVariant.CreatedAt,
            UpdatedAt = productVariant.UpdatedAt,
            CreatedById = productVariant.CreatedById,
            UpdatedById = productVariant.UpdatedById
        };
    }
}
