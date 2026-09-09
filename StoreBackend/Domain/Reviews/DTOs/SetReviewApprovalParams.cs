namespace Domain.Reviews;

public class SetReviewApprovalParams
{
    public required Guid ReviewId { get; set; }

    /// <summary>
    /// True publishes the review, false takes it back off the product page. Both directions
    /// are the same call, because moderation is not a one-way door: a review cleared in
    /// error has to be able to come back down.
    /// </summary>
    public required bool IsApproved { get; set; }

    public required string UpdatedById { get; set; }
}
