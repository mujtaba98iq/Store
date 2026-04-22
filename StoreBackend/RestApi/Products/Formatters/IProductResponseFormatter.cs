using Domain.Products;

namespace RestApi.Products;

public interface IProductResponseFormatter
{
    ProductResponse One(Product product);
    ProductListResponse Many(IEnumerable<Product> products, int totalCount);
}
