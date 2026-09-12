using RestApi.ErrorHandling;

namespace RestApi.Setup;

public static class ErrorHandlingSetup
{
    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Required: the parameterless UseExceptionHandler() refuses to start without a
        // last resort to fall back on, and throws while the pipeline is being built.
        // GlobalExceptionHandler answers everything, so this is only ever reached when
        // it stands aside - which it does once the response has already started, when
        // there is nothing left to write anyway.
        builder.Services.AddProblemDetails();

        return builder;
    }
}
