
using Domain.Products;
using RestApi.Categories;
using RestApi.ProductImages;
using RestApi.ProductVariants;

namespace RestApi.Products;

public class ProductResponseFormatter(ICategoryResponseFormatter categoryFormatter, IProductVariantResponseFormatter variantFormatter, IProductImageResponseFormatter imageFormatter) : IProductResponseFormatter
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
                : new List<CategoryResponse>(),
            Variants = product.Variants != null
                ? product.Variants.Select(variantFormatter.One).ToList()
                : new List<ProductVariantResponse>(),
            Images = product.Images != null
                ? product.Images.Select(imageFormatter.One).ToList()
                : new List<ProductImageResponse>()
        };
    }
}
