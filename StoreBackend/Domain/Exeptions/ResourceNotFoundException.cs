using Domain.Exeptions;

namespace Domain;

/// <summary>
/// Raised when a request names something that is not there.
/// </summary>
/// <param name="resource">
/// What was being looked for - "Order", "Product". Kept apart from the message so a
/// log can be read by resource without parsing the sentence.
/// </param>
/// <param name="message">
/// The whole sentence, written for whoever made the request: "Order with ID 3f2a
/// not found". It is what the API answers with, so it carries nothing from inside
/// the server beyond the identifier the caller sent in.
/// </param>
public class ResourceNotFoundException(string resource, string message) : DomainException(message)
{
    public string Resource { get; } = resource;
}
