using Sheard.Type;

namespace Domain.ProductImages;

public interface IProductImageService
{
    Task<ProductImage> Create(CreateProductImageParams createProductImageParams);
    Task<ProductImage?> FindById(Guid id);
    Task<ProductImage> Update(UpdateProductImageParams updateProductImageParams);
    Task<PaginationResult<ProductImage>> Search(ProductImageFilters productImageFilters);
}
