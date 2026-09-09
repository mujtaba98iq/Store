namespace RestApi.Reviews;

public class UpdateReviewRequest
{
    /// <summary>
    /// Both are optional, and whatever is left out keeps the value it had. The product and
    /// the order are not editable: a review is about one purchase and cannot be moved to
    /// another.
    /// </summary>
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}
