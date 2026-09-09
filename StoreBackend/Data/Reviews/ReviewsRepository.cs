using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Reviews;

public class ReviewsRepository(ApplicationDbContext dbContext) : IReviewsRepository
{
    public async Task<Review> Create(Review review)
    {
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();
        return review;
    }

    public async Task<List<Review>> FindByFilters(ReviewFilters reviewFilters)
    {
        var query = dbContext.Reviews.AsNoTracking()
            .Where(r => r.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, reviewFilters);
        query = ApplyOrdering(query, reviewFilters);
        query = ApplyPagination(query, reviewFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Review> ApplyPagination(IQueryable<Review> query, ReviewFilters reviewFilters)
    {
        var page = reviewFilters.Page <= 0 ? 1 : reviewFilters.Page;
        var pageSize = reviewFilters.PageSize <= 0 ? 10 : reviewFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Review> ApplyOrdering(IQueryable<Review> query, ReviewFilters reviewFilters)
    {
        var reviewOrderBy = reviewFilters.OrderBy ?? ReviewOrderBy.CreatedAt;
        var orderDirection = reviewFilters.OrderByDirection ?? OrderDirection.Desc;

        return reviewOrderBy switch
        {
            ReviewOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt),
            ReviewOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(r => r.UpdatedAt)
                : query.OrderByDescending(r => r.UpdatedAt),
            // Descending puts the best first, which is what a product page shows. Newest
            // first is still the default, a rating being a poor way to order a queue of
            // reviews staff have to work through.
            ReviewOrderBy.Rating => orderDirection == OrderDirection.Asc
                ? query.OrderBy(r => r.Rating)
                : query.OrderByDescending(r => r.Rating),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt)
        };
    }

    private static IQueryable<Review> ApplyFilters(IQueryable<Review> query, ReviewFilters reviewFilters)
    {
        if (reviewFilters.ReviewId != null)
        {
            query = query.Where(r => r.Id == reviewFilters.ReviewId);
        }

        if (reviewFilters.UserId != null)
        {
            query = query.Where(r => r.UserId == reviewFilters.UserId);
        }

        if (reviewFilters.ProductId != null)
        {
            query = query.Where(r => r.ProductId == reviewFilters.ProductId);
        }

        if (reviewFilters.OrderId != null)
        {
            query = query.Where(r => r.OrderId == reviewFilters.OrderId);
        }

        if (reviewFilters.IsApproved.HasValue)
        {
            query = query.Where(r => r.IsApproved == reviewFilters.IsApproved.Value);
        }

        if (reviewFilters.MinRating.HasValue)
        {
            query = query.Where(r => r.Rating >= reviewFilters.MinRating.Value);
        }

        if (reviewFilters.MaxRating.HasValue)
        {
            query = query.Where(r => r.Rating <= reviewFilters.MaxRating.Value);
        }

        if (reviewFilters.CreatedFrom.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= reviewFilters.CreatedFrom.Value);
        }

        if (reviewFilters.CreatedTo.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= reviewFilters.CreatedTo.Value);
        }

        return query;
    }

    public async Task<Review?> FindById(Guid id)
    {
        var review = await dbContext.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null);
        return review;
    }

    public async Task<Review?> FindByUserAndProduct(Guid userId, Guid productId)
    {
        var review = await dbContext.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId && r.DeletedAt == null);
        return review;
    }

    public async Task<Review> Update(Review review)
    {
        // Written straight to the row rather than through the change tracker, as payments
        // are. Only the stars, the words and the moderation flag move: who left the review,
        // what it is about and the order that entitled it are what the review is, and none of
        // them change.
        await dbContext.Reviews
            .Where(r => r.Id == review.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Rating, review.Rating)
                .SetProperty(r => r.Comment, review.Comment)
                .SetProperty(r => r.IsApproved, review.IsApproved)
                .SetProperty(r => r.UpdatedAt, review.UpdatedAt)
                .SetProperty(r => r.UpdatedById, review.UpdatedById)
                .SetProperty(r => r.DeletedAt, review.DeletedAt)
                .SetProperty(r => r.DeletedById, review.DeletedById));

        return review;
    }

    public async Task<int> GetTotalCountByFilters(ReviewFilters reviewFilters)
    {
        var query = dbContext.Reviews.AsNoTracking()
            .Where(r => r.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, reviewFilters);
        return await query.CountAsync();
    }
}
