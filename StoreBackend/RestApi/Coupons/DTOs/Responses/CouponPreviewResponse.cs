namespace RestApi.Coupons;

/// <summary>
/// What a coupon would come to on the customer's cart as it stands. A quotation, not a
/// reservation: nothing is spent, the campaign's usage limit does not move, and a code
/// previewed successfully can still be refused at checkout if it runs out in between.
/// </summary>
public class CouponPreviewResponse
{
    /// <summary>
    /// The code as stored rather than as typed, so a client can display it back the way it
    /// will appear on the order.
    /// </summary>
    public required string Code { get; set; }

    public required string DiscountType { get; set; }
    public required decimal DiscountValue { get; set; }

    /// <summary>
    /// The cart's goods, which is what the discount was worked out against. Returned so a
    /// client can show the customer the arithmetic rather than only its result.
    /// </summary>
    public required decimal Subtotal { get; set; }

    /// <summary>
    /// What would come off, already capped and already clamped to the subtotal.
    /// </summary>
    public required decimal DiscountAmount { get; set; }

    /// <summary>
    /// The goods after the discount. Carriage is added on top of this at checkout: a coupon
    /// has nothing to say about postage.
    /// </summary>
    public required decimal Total { get; set; }
}
