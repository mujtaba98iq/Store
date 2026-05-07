using Microsoft.Extensions.DependencyInjection;

namespace Domain.Categories;

public static class CategoriesModule
{
    public static IServiceCollection AddCategoriesModule(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    } 
}
