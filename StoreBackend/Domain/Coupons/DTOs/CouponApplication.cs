namespace Domain.Coupons;

/// <summary>
/// What a coupon comes to on a particular order. Returned rather than just the amount, so
/// the caller can record which coupon produced it: an order that kept only the number could
/// never say afterwards why the customer paid less.
/// </summary>
public class CouponApplication
{
    public required Coupon Coupon { get; init; }

    /// <summary>
    /// The goods the discount was worked out against, carried back so a preview can show the
    /// customer the arithmetic rather than only its result.
    /// </summary>
    public required decimal Subtotal { get; init; }

    /// <summary>
    /// What comes off. Already capped and already clamped to the subtotal, so a caller may
    /// subtract it without checking it again.
    /// </summary>
    public required decimal DiscountAmount { get; init; }

    /// <summary>
    /// The goods after the discount. Carriage is added on top of this by whoever is building
    /// the order; a coupon has nothing to say about postage.
    /// </summary>
    public decimal Total => Subtotal - DiscountAmount;
}
