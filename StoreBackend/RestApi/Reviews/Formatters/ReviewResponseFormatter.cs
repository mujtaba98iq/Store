using Domain.Reviews;

namespace RestApi.Reviews;

public class ReviewResponseFormatter : IReviewResponseFormatter
{
    public ReviewListResponse Many(IEnumerable<Review> reviews, int totalCount)
    {
        return new ReviewListResponse
        {
            Data = reviews.Select(One).ToList(),
            TotalCount = totalCount
        };
    }

    public ReviewResponse One(Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id.ToString(),
            UserId = review.UserId.ToString(),
            ProductId = review.ProductId.ToString(),
            OrderId = review.OrderId.ToString(),
            Rating = review.Rating,
            Comment = review.Comment,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            CreatedById = review.CreatedById,
            UpdatedById = review.UpdatedById
        };
    }
}
