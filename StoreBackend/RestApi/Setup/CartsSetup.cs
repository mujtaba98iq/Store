using Domain.Carts;
using RestApi.Carts;

namespace RestApi.Setup;

public static class CartsSetup
{
    public static WebApplicationBuilder AddCartsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddCartsModule();
        builder.Services.AddScoped<ICartResponseFormatter, CartResponseFormatter>();
        return builder;
    }
}
