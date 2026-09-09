namespace RestApi.Reviews;

public class SetReviewApprovalRequest
{
    /// <summary>
    /// True publishes the review on the product page, false takes it back down.
    /// </summary>
    public required bool IsApproved { get; set; }
}
