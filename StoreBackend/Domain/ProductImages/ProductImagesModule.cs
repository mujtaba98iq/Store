using Microsoft.Extensions.DependencyInjection;

namespace Domain.ProductImages;

public static class ProductImagesModule
{
    public static IServiceCollection AddProductImagesModule(this IServiceCollection services)
    {
        services.AddScoped<IProductImageService, ProductImageService>();
        return services;
    } 
}
