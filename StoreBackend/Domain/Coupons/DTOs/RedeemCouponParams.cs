namespace Domain.Coupons;

/// <summary>
/// A coupon being spent rather than weighed. Booked against the coupon's usage limit, so
/// unlike <see cref="ApplyCouponParams"/> this call changes something.
/// </summary>
public class RedeemCouponParams
{
    public required string Code { get; set; }
    public required decimal Subtotal { get; set; }
    public required string UpdatedById { get; set; }
}
