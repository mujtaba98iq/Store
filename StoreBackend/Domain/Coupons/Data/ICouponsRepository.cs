namespace Domain.Coupons;

public interface ICouponsRepository
{
    Task<Coupon> Create(Coupon coupon);
    Task<Coupon> Update(Coupon coupon);
    Task<Coupon?> FindById(Guid id);

    /// <summary>
    /// The lookup a checkout makes. The code is matched as stored, so callers pass it
    /// through <see cref="Coupon.NormaliseCode"/> first.
    /// </summary>
    Task<Coupon?> FindByCode(string code);

    Task<List<Coupon>> FindByFilters(CouponFilters couponFilters);
    Task<int> GetTotalCountByFilters(CouponFilters couponFilters);

    /// <summary>
    /// Books one redemption against the coupon, and says whether there was one to be had.
    ///
    /// The limit is re-checked as part of the write rather than by the caller beforehand,
    /// which is the only way two customers reaching for the last redemption at the same
    /// moment can be told apart. False means the coupon ran out between the caller's check
    /// and this call.
    /// </summary>
    Task<bool> TryConsume(Guid couponId);
}
