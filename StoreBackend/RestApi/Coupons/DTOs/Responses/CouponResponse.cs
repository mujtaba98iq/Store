namespace RestApi.Coupons;

public class CouponResponse
{
    public required string Id { get; set; }
    public required string Code { get; set; }

    /// <summary>
    /// Rendered by name rather than by number, so a client never has to carry a copy of the
    /// enum to make sense of it.
    /// </summary>
    public required string DiscountType { get; set; }

    public required decimal DiscountValue { get; set; }

    /// <summary>
    /// Null where the campaign has no floor and no ceiling respectively.
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }

    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    /// <summary>
    /// Null on an unlimited campaign, in which case <see cref="UsedCount"/> is a tally
    /// rather than a countdown.
    /// </summary>
    public int? UsageLimit { get; set; }

    public required int UsedCount { get; set; }

    /// <summary>
    /// The switch staff threw, on its own. It says nothing about the dates: an active coupon
    /// whose window has closed still shows true here.
    /// </summary>
    public required bool IsActive { get; set; }

    /// <summary>
    /// Whether a customer could actually redeem it this moment — switched on, inside its
    /// window, and with redemptions left. This is the one to render a badge from;
    /// <see cref="IsActive"/> on its own would call an expired campaign live.
    /// </summary>
    public required bool IsRedeemable { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
