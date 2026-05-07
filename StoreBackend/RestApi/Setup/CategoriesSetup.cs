using Domain.Categories;
using RestApi.Categories;

namespace RestApi.Setup;

public static class CategoriesSetup
{
    public static WebApplicationBuilder AddCategoriesModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddCategoriesModule();
        builder.Services.AddScoped<ICategoryResponseFormatter, CategoryResponseFormatter>();
        return builder;
    }
}
