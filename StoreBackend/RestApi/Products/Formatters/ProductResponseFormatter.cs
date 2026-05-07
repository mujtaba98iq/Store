
using Domain.Products;
using RestApi.Categories;

namespace RestApi.Products;

public class ProductResponseFormatter(ICategoryResponseFormatter categoryFormatter) : IProductResponseFormatter
{
    public ProductListResponse Many(IEnumerable<Product> product, int totalCount)
    {
        var productResults = product.Select(One).ToList();

        return new ProductListResponse
        {
            Data = productResults,
            TotalCount = totalCount
        };
    }

    public ProductResponse One(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            ImagePath = product.ImagePath,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CreatedById = product.CreatedById,
            UpdatedById = product.UpdatedById,
            Categories = product.Categories != null 
                ? product.Categories.Select(categoryFormatter.One).ToList() 
                : new List<CategoryResponse>()
        };
    }
}
