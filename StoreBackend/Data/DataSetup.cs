using Data.Auth;
using Data.Carts;
using Data.Categories;
using Data.Coupons;
using Data.Inventories;
using Data.Orders;
using Data.Payments;
using Data.ProductImages;
using Data.ProductVariants;
using Data.Products;
using Data.Reviews;
using Data.Shipments;
using Data.Users;
using Data.Wishlists;
using Domain.Auth;
using Domain.Carts;
using Domain.Categories;
using Domain.Coupons;
using Domain.Inventories;
using Domain.Orders;
using Domain.Payments;
using Domain.ProductImages;
using Domain.ProductVariants;
using Domain.Products;
using Domain.Reviews;
using Domain.Shipments;
using Domain.Users;
using Domain.Wishlists;
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
        services.AddScoped<ICouponsRepository, CouponsRepository>();
        services.AddScoped<IReviewsRepository, ReviewsRepository>();
        services.AddScoped<IWishlistItemsRepository, WishlistItemsRepository>();
        services.AddScoped<IUsersRepository,  UsersRepository>();
        services.AddScoped<IAuthRepository,   AuthRepository>();

        return services;
    }
}

