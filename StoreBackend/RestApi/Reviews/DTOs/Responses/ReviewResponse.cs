namespace RestApi.Reviews;

public class ReviewResponse
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string ProductId { get; set; }

    /// <summary>
    /// The purchase behind the review. It is what lets a client mark an entry as a verified
    /// purchase, every review having one by construction.
    /// </summary>
    public required string OrderId { get; set; }

    public required int Rating { get; set; }

    /// <summary>
    /// Null where the customer rated without writing anything.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Whether staff have published it. Always false on a freshly created or freshly edited
    /// review, so a client can tell the author their words are waiting rather than live.
    /// </summary>
    public required bool IsApproved { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
