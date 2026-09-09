using Sheard.Type;

namespace Domain.Reviews;

public class ReviewFilters : ListingOptions
{
    public Guid? ReviewId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ProductId { get; set; }

    /// <summary>
    /// The purchase a review came out of. Narrow, but it is what answers "what did this
    /// customer say about the order we sent them".
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// True keeps only what has been published, false only what is still waiting on staff.
    /// The false case is the moderation queue.
    /// </summary>
    public bool? IsApproved { get; set; }

    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }

    /// <summary>
    /// Bounds on when the review was left. Inclusive at both ends.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public ReviewOrderBy? OrderBy { get; set; } = ReviewOrderBy.CreatedAt;
}
