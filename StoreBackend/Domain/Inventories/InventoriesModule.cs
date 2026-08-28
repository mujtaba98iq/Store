using Microsoft.Extensions.DependencyInjection;

namespace Domain.Inventories;

public static class InventoriesModule
{
    public static IServiceCollection AddInventoriesModule(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
        return services;
    }
}
