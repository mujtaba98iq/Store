using System.Text.Json.Serialization;

namespace RestApi.ErrorHandling;

/// <summary>
/// The one shape every failed request answers with, whatever went wrong.
/// </summary>
/// <remarks>
/// Serialised camel-cased, so the body reads <c>{ "message": "..." }</c> - which is
/// the first thing the web client looks for, and the same shape the controllers
/// were already hand-writing before <see cref="GlobalExceptionHandler"/> took the
/// job over. Nothing else is ever added: no exception type, no stack, no headers.
/// </remarks>
public sealed class ApiError
{
    /// <summary>A sentence written for whoever made the request.</summary>
    public required string Message { get; init; }

    /// <summary>
    /// Field name to the messages against it, when the failure was a validation one.
    /// Left out entirely otherwise, so a client can treat its presence as the signal
    /// that there is something to mark on the form.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
