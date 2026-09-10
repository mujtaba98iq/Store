namespace Domain.Coupons;

/// <summary>
/// A coupon weighed against whatever is in the customer's cart right now.
///
/// The subtotal is read from the cart rather than accepted from the caller, so what the
/// customer is quoted is worked out from the same number checkout will use. A client that
/// could name its own subtotal could be told a discount the order would never honour.
/// </summary>
public class PreviewCouponParams
{
    public required Guid UserId { get; set; }
    public required string Code { get; set; }
}
