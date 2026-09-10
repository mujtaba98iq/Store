namespace Domain.Coupons;

public class UpdateCouponParams
{
    public required Guid CouponId { get; set; }

    /// <summary>
    /// Null leaves what is already there. The code itself is not among these: orders keep
    /// their own copy of the code they were placed with, so renaming a coupon would leave
    /// the two disagreeing about what the customer actually quoted. A code that was wrong is
    /// withdrawn and replaced.
    /// </summary>
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UsageLimit { get; set; }

    /// <summary>
    /// The way a campaign is stopped early or started again. Both directions are the same
    /// call: withdrawing a coupon is not a one-way door.
    /// </summary>
    public bool? IsActive { get; set; }

    public required string UpdatedById { get; set; }
}
