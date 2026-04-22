using Data.Auth;
using Data.Products;
using Data.Users;
using Domain.Auth;
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

        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IUsersRepository,  UsersRepository>();
        services.AddScoped<IAuthRepository,   AuthRepository>();

        return services;
    }
}

