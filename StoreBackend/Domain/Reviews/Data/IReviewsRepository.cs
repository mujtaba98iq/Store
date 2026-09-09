namespace Domain.Reviews;

public interface IReviewsRepository
{
    Task<Review> Create(Review review);
    Task<Review> Update(Review review);
    Task<Review?> FindById(Guid id);

    /// <summary>
    /// Used to spot a customer reviewing a product they have already had their say on, so
    /// the clash reads as the duplicate it is rather than as a database error.
    /// </summary>
    Task<Review?> FindByUserAndProduct(Guid userId, Guid productId);

    Task<List<Review>> FindByFilters(ReviewFilters reviewFilters);
    Task<int> GetTotalCountByFilters(ReviewFilters reviewFilters);
}
