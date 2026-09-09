namespace RestApi.Reviews;

public class CreateReviewRequest
{
    public required Guid ProductId { get; set; }

    /// <summary>
    /// The order the product was bought on. Asked for rather than worked out, because a
    /// customer who bought the same product twice is the only one who can say which purchase
    /// they are talking about.
    /// </summary>
    public required Guid OrderId { get; set; }

    /// <summary>
    /// One to five stars.
    /// </summary>
    public required int Rating { get; set; }

    /// <summary>
    /// Optional: a rating on its own is a review.
    /// </summary>
    public string? Comment { get; set; }
}
