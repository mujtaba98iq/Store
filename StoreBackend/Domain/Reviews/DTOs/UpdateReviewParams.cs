namespace Domain.Reviews;

public class UpdateReviewParams
{
    public required Guid ReviewId { get; set; }

    /// <summary>
    /// The customer doing the editing. A review is somebody's own words, so only its author
    /// may change them: staff moderate by approving or holding a review, never by rewriting
    /// it.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Null leaves what is already there. Rating and comment can be revised independently:
    /// changing your mind about the stars is not the same as changing what you wrote.
    /// </summary>
    public int? Rating { get; set; }
    public string? Comment { get; set; }

    public required string UpdatedById { get; set; }
}
