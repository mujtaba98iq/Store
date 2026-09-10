using Domain.Coupons;

namespace RestApi.Coupons;

public class UpdateCouponRequest
{
    /// <summary>
    /// Everything is optional, and whatever is left out keeps the value it had.
    ///
    /// The code is not here. Orders keep their own copy of what was quoted, so renaming a
    /// coupon would leave the coupon and the orders placed against it disagreeing about what
    /// the customer typed. A code that was wrong is switched off and replaced.
    /// </summary>
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Cannot be dropped below the number of redemptions already made: that would put the
    /// campaign permanently over its own ceiling. Switch the coupon off instead.
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// How a campaign is stopped early or started again.
    /// </summary>
    public bool? IsActive { get; set; }
}
