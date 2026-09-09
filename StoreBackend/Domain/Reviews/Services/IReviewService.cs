using Sheard.Type;

namespace Domain.Reviews;

public interface IReviewService
{
    /// <summary>
    /// Records a customer's verdict on something they bought. Refused unless the order given
    /// is theirs, was not called off, and actually held the product. Arrives unpublished:
    /// staff decide what goes on the product page.
    /// </summary>
    Task<Review> Create(CreateReviewParams createReviewParams);

    Task<Review?> FindById(Guid id);
    Task<PaginationResult<Review>> Search(ReviewFilters reviewFilters);

    /// <summary>
    /// Lets the author revise their own review. It goes back into the moderation queue,
    /// whatever it was cleared as before.
    /// </summary>
    Task<Review> Update(UpdateReviewParams updateReviewParams);

    /// <summary>
    /// Publishes a review or takes it back down. Staff-facing: this is the moderation
    /// decision, and it is the only part of a review staff own.
    /// </summary>
    Task<Review> SetApproval(SetReviewApprovalParams setReviewApprovalParams);
}
