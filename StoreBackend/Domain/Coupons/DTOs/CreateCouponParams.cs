namespace Domain.Coupons;

public class CreateCouponParams
{
    /// <summary>
    /// Taken as staff typed it and normalised on the way in, so the row is stored in the
    /// one form every later lookup uses.
    /// </summary>
    public required string Code { get; set; }

    public required DiscountType DiscountType { get; set; }
    public required decimal DiscountValue { get; set; }

    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// Both in UTC, and both inclusive. Required: a coupon that never expires is a standing
    /// price cut rather than a campaign, and should be entered as a long window rather than
    /// as no window at all.
    /// </summary>
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    /// <summary>
    /// Null for an unlimited campaign.
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// Defaults to live. A coupon staff want to line up ahead of time is created inactive,
    /// or given a window that has not opened yet.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public required string CreatedById { get; set; }
}
