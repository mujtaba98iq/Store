namespace Domain.Products;

public interface IProductsRepository
{
    Task<Product> Create(Product Product);
    Task<Product> Update(Product Product);
    Task<Product?> FindById(Guid id);
    Task<List<Product>> FindByFilters(ProductFilters productFilters);
    Task<int> GetTotalCountByFilters(ProductFilters productFilters);
}
