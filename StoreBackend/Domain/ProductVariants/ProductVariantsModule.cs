using Microsoft.Extensions.DependencyInjection;

namespace Domain.ProductVariants;

public static class ProductVariantsModule
{
    public static IServiceCollection AddProductVariantsModule(this IServiceCollection services)
    {
        services.AddScoped<IProductVariantService, ProductVariantService>();
        return services;
    } 
}
