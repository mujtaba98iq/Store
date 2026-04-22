using Sheard.Type;

namespace Domain.Products
{
    public class ProductService(IProductsRepository productsRepository) : IProductService
    {
        public async Task<Product> Create(CreateProductParams CreateProductParams)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = CreateProductParams.Name,
                Description = CreateProductParams.Description,
                Price = CreateProductParams.Price,
                Quantity = CreateProductParams.Quantity,
                ImagePath = CreateProductParams.ImagePath,
                CreatedAt = DateTime.UtcNow,
                CreatedById = CreateProductParams.CreatedById
            };

            product.CreatedById = "1";
            return await productsRepository.Create(product);
        }

        public async Task<Product?> FindById(Guid id)
        {
            return await productsRepository.FindById(id);
        }

        public async Task<PaginationResult<Product>> Search(ProductFilters ProductFilters)
        {
            var products = await productsRepository.FindByFilters(ProductFilters);
            var totalCount = await productsRepository.GetTotalCountByFilters(ProductFilters);

            return new PaginationResult<Product>
            {
                TotalCount = totalCount,
                Data = products
            };
        }

        public async Task<Product> Update(UpdateProductParams udpateProductParams)
        {
            var product = await productsRepository.FindById(udpateProductParams.Id) ?? throw new ResourceNotFoundException("Product", $"Product with ID {udpateProductParams.Id} not found");

            product.Name = udpateProductParams.Name ?? product.Name;
            product.Description = udpateProductParams.Description ?? product.Description;
            product.Price = udpateProductParams.Price ?? product.Price;
            product.Quantity = udpateProductParams.Quantity ?? product.Quantity;
            product.ImagePath = udpateProductParams.ImagePath ?? product.ImagePath;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedById = "1";

            return await productsRepository.Update(product);
        } 
    }
}
