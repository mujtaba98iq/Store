using Domain.Shipments;
using RestApi.Shipments;

namespace RestApi.Setup;

public static class ShipmentsSetup
{
    public static WebApplicationBuilder AddShipmentsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddShipmentsModule();
        builder.Services.AddScoped<IShipmentResponseFormatter, ShipmentResponseFormatter>();
        return builder;
    }
}
