using Microsoft.Extensions.DependencyInjection;

namespace Domain.Carts;

public static class CartsModule
{
    public static IServiceCollection AddCartsModule(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        return services;
    }
}
