using Data.Auth;
using Data.Categories;
using Data.ProductImages;
using Data.ProductVariants;
using Data.Products;
using Data.Users;
using Domain.Auth;
using Domain.Categories;
using Domain.ProductImages;
using Domain.ProductVariants;
using Domain.Products;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

public static class DataSetup
{
    public static IServiceCollection AddData(this IServiceCollection services, DatabaseSettings databaseSettings, bool isDevelopment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(databaseSettings.ConnectionString)
            .EnableSensitiveDataLogging(isDevelopment)
        );

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IProductVariantsRepository, ProductVariantsRepository>();
        services.AddScoped<IProductImagesRepository, ProductImagesRepository>();
        services.AddScoped<IUsersRepository,  UsersRepository>();
        services.AddScoped<IAuthRepository,   AuthRepository>();

        return services;
    }
}

