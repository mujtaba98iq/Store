using Data.Auth;
using Data.Carts;
using Data.Categories;
using Data.Inventories;
using Data.Orders;
using Data.Payments;
using Data.ProductImages;
using Data.ProductVariants;
using Data.Products;
using Data.Shipments;
using Data.Users;
using Domain.Auth;
using Domain.Carts;
using Domain.Categories;
using Domain.Inventories;
using Domain.Orders;
using Domain.Payments;
using Domain.ProductImages;
using Domain.ProductVariants;
using Domain.Products;
using Domain.Shipments;
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

        services.AddScoped<ICartsRepository, CartsRepository>();
        services.AddScoped<ICartItemsRepository, CartItemsRepository>();
        services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IProductVariantsRepository, ProductVariantsRepository>();
        services.AddScoped<IProductImagesRepository, ProductImagesRepository>();
        services.AddScoped<IInventoriesRepository, InventoriesRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IOrderItemsRepository, OrderItemsRepository>();
        services.AddScoped<IPaymentsRepository, PaymentsRepository>();
        services.AddScoped<IShipmentsRepository, ShipmentsRepository>();
        services.AddScoped<IUsersRepository,  UsersRepository>();
        services.AddScoped<IAuthRepository,   AuthRepository>();

        return services;
    }
}

