namespace Domain.Coupons;

/// <summary>
/// How a coupon's <see cref="Coupon.DiscountValue"/> should be read. The values are
/// persisted as numbers, so they may be added to but never renumbered.
/// </summary>
public enum DiscountType
{
    /// <summary>
    /// A share of the order, so what comes off grows with the basket. This is the type
    /// <see cref="Coupon.MaximumDiscountAmount"/> exists for: without a ceiling, a percentage
    /// off an unusually large order gives away an unbounded amount of money.
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// A flat sum off, whatever the order comes to. Paired with
    /// <see cref="Coupon.MinimumOrderAmount"/> in practice, a fixed amount being worth more
    /// on a small basket than on a large one.
    /// </summary>
    FixedAmount = 2,
}
