namespace Domain.Coupons;

/// <summary>
/// A coupon weighed against an order of a known size, without anything being spent. Used
/// both for the preview a customer sees before committing and, a moment later, for the
/// real thing at checkout.
/// </summary>
public class ApplyCouponParams
{
    public required string Code { get; set; }

    /// <summary>
    /// The goods the discount is worked out against, carriage excluded. A coupon takes money
    /// off what is being bought, not off what it costs to post it, and the minimum is judged
    /// on the same number for the same reason.
    /// </summary>
    public required decimal Subtotal { get; set; }
}
