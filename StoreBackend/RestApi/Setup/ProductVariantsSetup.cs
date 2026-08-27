using Domain.ProductVariants;
using RestApi.ProductVariants;

namespace RestApi.Setup;

public static class ProductVariantsSetup
{
    public static WebApplicationBuilder AddProductVariantsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddProductVariantsModule();
        builder.Services.AddScoped<IProductVariantResponseFormatter, ProductVariantResponseFormatter>();
        return builder;
    }
}
