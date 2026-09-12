using System.Diagnostics;
using Domain.Exeptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace RestApi.ErrorHandling;

/// <summary>
/// The one place a request that threw is turned into a response.
/// </summary>
/// <remarks>
/// <para>
/// Registered through <c>app.UseExceptionHandler()</c> at the very front of the
/// pipeline, so it sits outside every controller and no action needs a
/// <c>try</c>/<c>catch</c> of its own. It also sits inside the developer exception
/// page that <see cref="WebApplication"/> adds in Development: handling the
/// exception here means that page is never reached, in any environment, and the
/// stack, the source paths and the request headers it renders - the bearer token
/// among them - can never be written to a response.
/// </para>
/// <para>
/// The split that matters is between a rule the API broke on purpose and a failure
/// nobody planned for. The first kind is a <see cref="DomainException"/>, whose
/// message was written for the caller and is answered verbatim. Everything else is
/// a fault: the caller is told only that one happened, and every detail goes to the
/// log behind a trace id that ties the two together.
/// </para>
/// </remarks>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string UnexpectedMessage = "An unexpected error occurred.";

    /// <summary>Names the log line that holds what this response would not say.</summary>
    public const string TraceHeader = "X-Trace-Id";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // The caller hung up. There is nobody to answer, and the attempt would only
        // throw again on a socket that is already gone.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        // Something is already on the wire, so the status line is spent and a clean
        // body cannot be substituted for what went out. Letting it through aborts
        // the response, which tells the client more honestly that it is incomplete.
        if (httpContext.Response.HasStarted)
        {
            logger.LogError(
                exception,
                "Unhandled exception after the response had started on {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            return false;
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var (status, error) = Describe(exception);

        Log(exception, httpContext, status, traceId);

        httpContext.Response.StatusCode = status;

        // The one link back to the log line, and it rides in a header rather than in
        // the body: the body is read as the message to show, and a second string in
        // there would be picked up as part of it.
        if (status >= StatusCodes.Status500InternalServerError)
        {
            httpContext.Response.Headers[TraceHeader] = traceId;
        }

        await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);
        return true;
    }

    /// <summary>
    /// Chooses the status and builds the body. Every branch that is not a rule the
    /// API broke on purpose writes its own sentence rather than the exception's, so
    /// no message from outside this method can reach a caller.
    /// </summary>
    private static (int Status, ApiError Error) Describe(Exception exception) =>
        exception switch
        {
            // Field-level messages, keyed by the field they belong to, so a form can
            // mark the box that is wrong instead of only showing a sentence.
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                new ApiError
                {
                    Message = string.Join(' ', validation.Errors.Select(failure => failure.ErrorMessage)),
                    Errors = validation.Errors
                        .GroupBy(failure => failure.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(failure => failure.ErrorMessage).ToArray()),
                }),

            // Written for the caller by the rule that raised it - see DomainException.
            DomainException domain => (
                DomainExceptionStatusMap.StatusFor(domain),
                new ApiError { Message = domain.Message }),

            // One sentence for every way of failing to authenticate, so that whether
            // a username exists cannot be read off the reply.
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                new ApiError { Message = "Invalid username or password." }),

            // The store named itself and quoted its own error; neither is the
            // caller's business, so only the fact that it failed goes back.
            ImageStorageException => (
                StatusCodes.Status502BadGateway,
                new ApiError { Message = "The image could not be stored. Please try again." }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ApiError { Message = UnexpectedMessage }),
        };

    /// <summary>
    /// Records the method, the path and the trace id - never the query string, the
    /// headers or the body, any of which can carry a token or a password.
    /// </summary>
    private void Log(Exception exception, HttpContext httpContext, int status, string traceId)
    {
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path} answered with {Status}. TraceId {TraceId}",
                method,
                path,
                status,
                traceId);
            return;
        }

        // A refused request is the API working, not failing, so it is recorded
        // without the stack that would make it read like a fault.
        logger.LogInformation(
            "{Method} {Path} refused with {Status}: {Reason}",
            method,
            path,
            status,
            exception.Message);
    }
}
