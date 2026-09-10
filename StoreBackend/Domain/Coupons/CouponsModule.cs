using Microsoft.Extensions.DependencyInjection;

namespace Domain.Coupons;

public static class CouponsModule
{
    public static IServiceCollection AddCouponsModule(this IServiceCollection services)
    {
        services.AddScoped<ICouponService, CouponService>();
        return services;
    }
}
