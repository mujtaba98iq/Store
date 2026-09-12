namespace Domain.Exeptions;

/// <summary>
/// Raised when a rating falls outside the star scale, such as a zero or a six.
/// </summary>
public class InvalidReviewRatingException(string message) : DomainException(message);
