namespace RestApi.Coupons;

public class PreviewCouponRequest
{
    /// <summary>
    /// The code the customer typed, in whatever case they typed it.
    ///
    /// The order it is weighed against is not asked for: the subtotal is read from the
    /// caller's own cart, so what they are quoted here is worked out from the same number
    /// checkout will use. A request that could name its own subtotal could be told a
    /// discount the order would never honour.
    /// </summary>
    public required string Code { get; set; }
}
