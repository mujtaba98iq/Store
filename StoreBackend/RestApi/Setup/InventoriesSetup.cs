using Domain.Inventories;
using RestApi.Inventories;

namespace RestApi.Setup;

public static class InventoriesSetup
{
    public static WebApplicationBuilder AddInventoriesModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddInventoriesModule();
        builder.Services.AddScoped<IInventoryResponseFormatter, InventoryResponseFormatter>();
        return builder;
    }
}
