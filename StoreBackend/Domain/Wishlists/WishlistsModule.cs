using Microsoft.Extensions.DependencyInjection;

namespace Domain.Wishlists;

public static class WishlistsModule
{
    public static IServiceCollection AddWishlistsModule(this IServiceCollection services)
    {
        services.AddScoped<IWishlistService, WishlistService>();
        return services;
    }
}
