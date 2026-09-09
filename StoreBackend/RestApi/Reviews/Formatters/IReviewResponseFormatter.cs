using Domain.Reviews;

namespace RestApi.Reviews;

public interface IReviewResponseFormatter
{
    ReviewResponse One(Review review);
    ReviewListResponse Many(IEnumerable<Review> reviews, int totalCount);
}
