using Microsoft.Extensions.DependencyInjection;

namespace Domain.Shipments;

public static class ShipmentsModule
{
    public static IServiceCollection AddShipmentsModule(this IServiceCollection services)
    {
        services.AddScoped<IShipmentService, ShipmentService>();
        return services;
    }
}
