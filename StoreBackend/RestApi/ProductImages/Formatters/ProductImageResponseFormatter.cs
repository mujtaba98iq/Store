using Domain.ProductImages;

namespace RestApi.ProductImages;

public class ProductImageResponseFormatter : IProductImageResponseFormatter
{
    public ProductImageListResponse Many(IEnumerable<ProductImage> productImages, int totalCount)
    {
        var productImageResults = productImages.Select(One).ToList();

        return new ProductImageListResponse
        {
            Data = productImageResults,
            TotalCount = totalCount
        };
    }

    public ProductImageResponse One(ProductImage productImage)
    {
        return new ProductImageResponse
        {
            Id = productImage.Id.ToString(),
            ProductId = productImage.ProductId.ToString(),
            ImageUrl = productImage.ImageUrl,
            IsPrimary = productImage.IsPrimary,
            DisplayOrder = productImage.DisplayOrder,
            CreatedAt = productImage.CreatedAt,
            UpdatedAt = productImage.UpdatedAt,
            CreatedById = productImage.CreatedById,
            UpdatedById = productImage.UpdatedById
        };
    }
}
