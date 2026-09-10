using Domain.Coupons;

namespace RestApi.Coupons;

public class CreateCouponRequest
{
    /// <summary>
    /// What customers will type. Stored upper-cased and trimmed, so the case entered here
    /// makes no difference to what will be accepted at checkout.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Percentage or FixedAmount, which decides how <see cref="DiscountValue"/> is read.
    /// </summary>
    public required DiscountType DiscountType { get; set; }

    /// <summary>
    /// A percentage between nought and a hundred, or an amount of money, according to the
    /// type above.
    /// </summary>
    public required decimal DiscountValue { get; set; }

    /// <summary>
    /// The smallest order that qualifies, judged on the goods rather than the total. Leave
    /// out for no floor.
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// The most that may ever come off. Leave out for no ceiling — but on a percentage
    /// coupon, a ceiling is what stops an unusually large order giving away an unbounded
    /// amount.
    /// </summary>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// The window the campaign runs in, both ends inclusive and both read as UTC.
    /// </summary>
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    /// <summary>
    /// How many redemptions the campaign allows in total, across every customer. Leave out
    /// for unlimited. It is not an allowance per customer: nothing stops one person using
    /// the code on several orders.
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// Defaults to live. Set false to load a campaign in ahead of time and switch it on
    /// later.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
