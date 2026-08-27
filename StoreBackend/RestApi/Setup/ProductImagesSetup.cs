using Domain.ProductImages;
using RestApi.ProductImages;

namespace RestApi.Setup;

public static class ProductImagesSetup
{
    public static WebApplicationBuilder AddProductImagesModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddProductImagesModule();
        builder.Services.AddScoped<IProductImageResponseFormatter, ProductImageResponseFormatter>();
        return builder;
    }
}
