using Sheard.Type;

namespace Domain.Products
{
    public interface IProductService
    {
        Task<Product?> FindById(Guid id);
        Task<Product> Create(CreateProductParams CreateProductParams);
        Task<Product> Update(UpdateProductParams UdpateProductParams);
        Task<PaginationResult<Product>> Search(ProductFilters ProductFilters);
    }
}
