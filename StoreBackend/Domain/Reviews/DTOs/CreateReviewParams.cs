namespace Domain.Reviews;

public class CreateReviewParams
{
    /// <summary>
    /// Whose opinion this is. Kept apart from <see cref="CreatedById"/> so the two stay
    /// honest if staff ever record a review on a customer's behalf.
    /// </summary>
    public required Guid UserId { get; set; }

    public required Guid ProductId { get; set; }

    /// <summary>
    /// The order the product was bought on. Required, because it is what the entitlement to
    /// review is checked against.
    /// </summary>
    public required Guid OrderId { get; set; }

    public required int Rating { get; set; }
    public string? Comment { get; set; }

    public required string CreatedById { get; set; }
}
