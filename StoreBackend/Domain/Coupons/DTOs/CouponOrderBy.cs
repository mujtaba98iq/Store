namespace Domain.Coupons;

public enum CouponOrderBy
{
    CreatedAt = 1,
    UpdatedAt = 2,
    Code = 3,

    /// <summary>
    /// What the campaign is worth, read as the raw number rather than as money: a fifteen
    /// percent coupon and a fifteen dinar one sort together, which is why this is only
    /// useful alongside a filter on the type.
    /// </summary>
    DiscountValue = 4,

    /// <summary>
    /// Busiest first, which is the read that says which campaign is actually working.
    /// </summary>
    UsedCount = 5,

    /// <summary>
    /// Closing soonest, for the campaigns that need a decision before they lapse.
    /// </summary>
    EndDate = 6,
}
