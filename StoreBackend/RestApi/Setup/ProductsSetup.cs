using Domain.Products;
using RestApi.Products;
namespace RestApi.Setup;

public static class ProductsSetup
{
    public static WebApplicationBuilder AddProductsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddProductsModule();
        builder.Services.AddScoped<IProductResponseFormatter, ProductResponseFormatter>();
        return builder;
    }
}
