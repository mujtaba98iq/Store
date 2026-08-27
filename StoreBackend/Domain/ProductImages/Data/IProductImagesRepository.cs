namespace Domain.ProductImages;

public interface IProductImagesRepository
{
    Task<ProductImage> Create(ProductImage productImage);
    Task<ProductImage> Update(ProductImage productImage);
    Task<ProductImage?> FindById(Guid id);
    Task<ProductImage?> FindPrimaryByProductId(Guid productId);
    Task<List<ProductImage>> FindByFilters(ProductImageFilters productImageFilters);
    Task<int> GetTotalCountByFilters(ProductImageFilters productImageFilters);
}
