namespace Domain.Exeptions;

/// <summary>
/// Raised when a customer reviews a product they have already had their say on. The review
/// they already hold is the one to revise.
/// </summary>
public class ProductAlreadyReviewedException(string message) : DomainException(message);
