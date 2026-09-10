using Domain.Data;

namespace Domain.Coupons;

/// <summary>
/// A code a customer can quote at checkout to have money taken off the order.
///
/// A coupon is a rule rather than a record of anything that happened: it says what comes
/// off, how large an order has to be to qualify, and for how long it stands. What it does
/// not say is who used it. The order keeps that, by pointing back here and by carrying its
/// own copy of the code, so the coupon can later be edited or withdrawn without rewriting
/// the history of the orders it was applied to.
/// </summary>
public class Coupon : IAuditableEntity
{
    /// <summary>
    /// The upper bound on a percentage coupon. Held here rather than in the service so the
    /// domain rule and the request validation cannot drift apart, the same way the star
    /// scale sits on a review.
    /// </summary>
    public const decimal MaxPercentageValue = 100m;

    /// <summary>
    /// Money is rounded to two places wherever a percentage produces a fraction of one.
    /// </summary>
    public const int MoneyDecimals = 2;

    public Guid Id { get; set; }

    /// <summary>
    /// What the customer types in. Held upper-cased and trimmed, and unique on those terms,
    /// so "save10", "SAVE10" and " SAVE10 " are one coupon rather than three: a customer
    /// reading a code off a poster should not have to reproduce its capitalisation.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Whether <see cref="DiscountValue"/> is a share of the order or a flat sum.
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// The size of the discount, read according to <see cref="DiscountType"/>: a percentage
    /// between nought and a hundred, or an amount of money. One column for both, because a
    /// coupon is only ever one or the other and a pair of nullable columns would allow a row
    /// that is neither.
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// The smallest order the coupon may be used on, checked against the goods rather than
    /// the total: whether an order qualifies should not depend on how far it is being
    /// posted. Null where there is no floor.
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// A ceiling on what may come off, whatever the arithmetic works out to. It is what makes
    /// a percentage coupon safe to put on a poster. Null where there is no cap.
    /// </summary>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// The window the coupon stands in, both ends inclusive and both in UTC. A coupon
    /// outside its window is refused without being touched: a campaign that has run its
    /// course has not been used up, it has simply ended, and the same row can be given a new
    /// window later.
    /// </summary>
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>
    /// How many times the coupon may be redeemed across every customer. Null means no limit,
    /// which is the ordinary case for a public campaign code.
    ///
    /// This is a total, not an allowance per customer: nothing here stops one person
    /// redeeming a code repeatedly on separate orders. Enforcing that needs a record of who
    /// redeemed what, which is a table this entity deliberately does not stand in for.
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// How many orders have actually been placed against the coupon. Incremented at checkout
    /// by a single conditional write, so two customers racing for the last redemption cannot
    /// both win it.
    ///
    /// It is not decremented when an order is cancelled. A redemption is a use of the offer
    /// rather than a reservation against it, and handing the code back would let a customer
    /// exhaust a limited campaign by placing and cancelling orders.
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// The switch staff use to withdraw a coupon early, independent of its dates. Kept apart
    /// from the window so a campaign can be stopped without losing when it was meant to run,
    /// and started again without rewriting its dates.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    /// <summary>
    /// Whether the coupon may be redeemed at all right now, leaving aside the order it would
    /// be used on. Withdrawn coupons and ones outside their window both fail here.
    /// </summary>
    public bool IsRedeemableAt(DateTime moment)
    {
        return IsActive && moment >= StartDate && moment <= EndDate;
    }

    /// <summary>
    /// Whether there are redemptions left. Always true where no limit was set.
    /// </summary>
    public bool HasRedemptionsLeft()
    {
        return UsageLimit == null || UsedCount < UsageLimit.Value;
    }

    /// <summary>
    /// What this coupon takes off an order of the given size.
    ///
    /// The cap is applied to a fixed amount as well as to a percentage. It is redundant
    /// there, a flat sum already being bounded, but applying it to whichever type the row
    /// happens to hold is one rule rather than two, and a cap that quietly did nothing on
    /// half the rows would be a trap for whoever set it.
    ///
    /// Clamped to the subtotal last of all, so a discount can never bill the customer a
    /// negative amount for the goods.
    /// </summary>
    public decimal CalculateDiscountFor(decimal subtotal)
    {
        var discount = DiscountType == DiscountType.Percentage
            ? Math.Round(subtotal * (DiscountValue / MaxPercentageValue), MoneyDecimals, MidpointRounding.AwayFromZero)
            : DiscountValue;

        if (MaximumDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, MaximumDiscountAmount.Value);
        }

        return Math.Clamp(discount, decimal.Zero, subtotal);
    }

    /// <summary>
    /// Puts a code into the form the column is unique on. Everything that reads or writes a
    /// code goes through here, so a lookup and the row it is looking for cannot disagree
    /// about capitalisation.
    /// </summary>
    public static string NormaliseCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }
}
