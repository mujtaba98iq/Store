
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Products;

public static class ProductsModule
{
    public static IServiceCollection AddProductsModule(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    } 
}
