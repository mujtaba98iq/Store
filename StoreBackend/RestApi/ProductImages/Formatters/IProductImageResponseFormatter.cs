using Domain.ProductImages;

namespace RestApi.ProductImages;

public interface IProductImageResponseFormatter
{
    ProductImageResponse One(ProductImage productImage);
    ProductImageListResponse Many(IEnumerable<ProductImage> productImages, int totalCount);
}
