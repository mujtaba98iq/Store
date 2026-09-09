using Domain.Wishlists;
using RestApi.Wishlists;

namespace RestApi.Setup;

public static class WishlistsSetup
{
    public static WebApplicationBuilder AddWishlistsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddWishlistsModule();
        builder.Services.AddScoped<IWishlistItemResponseFormatter, WishlistItemResponseFormatter>();
        return builder;
    }
}
