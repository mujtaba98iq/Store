using Domain.Coupons;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Coupons;

public class CouponsRepository(ApplicationDbContext dbContext) : ICouponsRepository
{
    public async Task<Coupon> Create(Coupon coupon)
    {
        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync();
        return coupon;
    }

    public async Task<List<Coupon>> FindByFilters(CouponFilters couponFilters)
    {
        var query = dbContext.Coupons.AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, couponFilters);
        query = ApplyOrdering(query, couponFilters);
        query = ApplyPagination(query, couponFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Coupon> ApplyPagination(IQueryable<Coupon> query, CouponFilters couponFilters)
    {
        var page = couponFilters.Page <= 0 ? 1 : couponFilters.Page;
        var pageSize = couponFilters.PageSize <= 0 ? 10 : couponFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Coupon> ApplyOrdering(IQueryable<Coupon> query, CouponFilters couponFilters)
    {
        var couponOrderBy = couponFilters.OrderBy ?? CouponOrderBy.CreatedAt;
        var orderDirection = couponFilters.OrderByDirection ?? OrderDirection.Desc;

        return couponOrderBy switch
        {
            CouponOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt),
            CouponOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.UpdatedAt)
                : query.OrderByDescending(c => c.UpdatedAt),
            CouponOrderBy.Code => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.Code)
                : query.OrderByDescending(c => c.Code),
            // The raw number, not what it is worth: a percentage and a fixed amount sort
            // against each other here, which is why this is only useful next to a filter on
            // the type.
            CouponOrderBy.DiscountValue => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.DiscountValue)
                : query.OrderByDescending(c => c.DiscountValue),
            CouponOrderBy.UsedCount => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.UsedCount)
                : query.OrderByDescending(c => c.UsedCount),
            CouponOrderBy.EndDate => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.EndDate)
                : query.OrderByDescending(c => c.EndDate),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };
    }

    private static IQueryable<Coupon> ApplyFilters(IQueryable<Coupon> query, CouponFilters couponFilters)
    {
        if (couponFilters.CouponId != null)
        {
            query = query.Where(c => c.Id == couponFilters.CouponId);
        }

        if (!string.IsNullOrWhiteSpace(couponFilters.Code))
        {
            // Normalised before the comparison, so a staff member searching in lower case
            // finds the row that is stored in upper.
            var code = Coupon.NormaliseCode(couponFilters.Code);
            query = query.Where(c => c.Code == code);
        }

        if (couponFilters.DiscountType.HasValue)
        {
            query = query.Where(c => c.DiscountType == couponFilters.DiscountType.Value);
        }

        if (couponFilters.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == couponFilters.IsActive.Value);
        }

        if (couponFilters.IsRedeemable.HasValue)
        {
            // The same three conditions the service checks before letting a coupon through,
            // written as a query so a staff list and a checkout cannot disagree about which
            // campaigns are live. The moment is taken once and sent as a parameter, so the
            // page of rows and the count beside it are judged against the same instant.
            var now = DateTime.UtcNow;

            query = couponFilters.IsRedeemable.Value
                ? query.Where(c => c.IsActive
                                   && c.StartDate <= now
                                   && c.EndDate >= now
                                   && (c.UsageLimit == null || c.UsedCount < c.UsageLimit))
                : query.Where(c => !c.IsActive
                                   || c.StartDate > now
                                   || c.EndDate < now
                                   || (c.UsageLimit != null && c.UsedCount >= c.UsageLimit));
        }

        // Overlap rather than containment: a campaign that started before the range asked
        // about and is still running is one that was live during it.
        if (couponFilters.ActiveFrom.HasValue)
        {
            query = query.Where(c => c.EndDate >= couponFilters.ActiveFrom.Value);
        }

        if (couponFilters.ActiveTo.HasValue)
        {
            query = query.Where(c => c.StartDate <= couponFilters.ActiveTo.Value);
        }

        if (couponFilters.CreatedFrom.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= couponFilters.CreatedFrom.Value);
        }

        if (couponFilters.CreatedTo.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= couponFilters.CreatedTo.Value);
        }

        return query;
    }

    public async Task<Coupon?> FindById(Guid id)
    {
        var coupon = await dbContext.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        return coupon;
    }

    public async Task<Coupon?> FindByCode(string code)
    {
        var coupon = await dbContext.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code && c.DeletedAt == null);
        return coupon;
    }

    public async Task<Coupon> Update(Coupon coupon)
    {
        // Written straight to the row rather than through the change tracker, as reviews and
        // orders are. UsedCount is deliberately not among the columns: it moves only through
        // TryConsume, so an edit that was loaded before a redemption cannot write the older
        // count back over it.
        await dbContext.Coupons
            .Where(c => c.Id == coupon.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.DiscountType, coupon.DiscountType)
                .SetProperty(c => c.DiscountValue, coupon.DiscountValue)
                .SetProperty(c => c.MinimumOrderAmount, coupon.MinimumOrderAmount)
                .SetProperty(c => c.MaximumDiscountAmount, coupon.MaximumDiscountAmount)
                .SetProperty(c => c.StartDate, coupon.StartDate)
                .SetProperty(c => c.EndDate, coupon.EndDate)
                .SetProperty(c => c.UsageLimit, coupon.UsageLimit)
                .SetProperty(c => c.IsActive, coupon.IsActive)
                .SetProperty(c => c.UpdatedAt, coupon.UpdatedAt)
                .SetProperty(c => c.UpdatedById, coupon.UpdatedById)
                .SetProperty(c => c.DeletedAt, coupon.DeletedAt)
                .SetProperty(c => c.DeletedById, coupon.DeletedById));

        return coupon;
    }

    public async Task<bool> TryConsume(Guid couponId)
    {
        // One statement, and the whole point of it: the limit is in the WHERE clause and the
        // increment reads the column rather than a value carried in from C#. Postgres settles
        // two concurrent redemptions of the last unit by letting exactly one of them match,
        // so the loser gets nought rows back rather than a count that went one too far.
        var affected = await dbContext.Coupons
            .Where(c => c.Id == couponId
                        && c.DeletedAt == null
                        && (c.UsageLimit == null || c.UsedCount < c.UsageLimit))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.UsedCount, c => c.UsedCount + 1));

        return affected == 1;
    }

    public async Task<int> GetTotalCountByFilters(CouponFilters couponFilters)
    {
        var query = dbContext.Coupons.AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, couponFilters);
        return await query.CountAsync();
    }
}
