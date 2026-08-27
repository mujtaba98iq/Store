using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace RestApi.Setup;

public static class OpenApiSetup
{
    private const string BearerSchemeName = "Bearer";

    public static WebApplicationBuilder AddOpenApiDocs(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the access token returned by POST /api/Auth/login. The \"Bearer\" prefix is added automatically."
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                bool requiresAuth = metadata.OfType<IAuthorizeData>().Any()
                                    && !metadata.OfType<IAllowAnonymous>().Any();

                if (requiresAuth)
                {
                    operation.Security =
                    [
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference(BearerSchemeName, context.Document)] = []
                        }
                    ];
                }

                return Task.CompletedTask;
            });
        });

        return builder;
    }

    public static WebApplication MapApiReference(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options
                .AddPreferredSecuritySchemes(BearerSchemeName)
                .EnablePersistentAuthentication();
        });

        return app;
    }
}
