using Domain.Coupons;
using RestApi.Coupons;

namespace RestApi.Setup;

public static class CouponsSetup
{
    public static WebApplicationBuilder AddCouponsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddCouponsModule();
        builder.Services.AddScoped<ICouponResponseFormatter, CouponResponseFormatter>();
        return builder;
    }
}
