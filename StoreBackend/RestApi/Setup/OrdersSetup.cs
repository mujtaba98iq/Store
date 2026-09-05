using Domain.Orders;
using RestApi.Orders;

namespace RestApi.Setup;

public static class OrdersSetup
{
    public static WebApplicationBuilder AddOrdersModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddOrdersModule();
        builder.Services.AddScoped<IOrderResponseFormatter, OrderResponseFormatter>();
        return builder;
    }
}
