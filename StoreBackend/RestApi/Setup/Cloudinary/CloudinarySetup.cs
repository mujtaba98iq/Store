using Domain.Storage;
using RestApi.Configration;

namespace RestApi.Setup;

public static class CloudinarySetup
{
    public static WebApplicationBuilder AddCloudinary(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(ConfigurationKeys.CloudinarySection));
        builder.Services.AddSingleton<IImageStorageService, CloudinaryImageStorageService>();

        return builder;
    }
}
