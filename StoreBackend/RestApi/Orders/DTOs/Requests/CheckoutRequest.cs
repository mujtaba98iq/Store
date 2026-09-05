namespace RestApi.Orders;

public class CheckoutRequest
{
    /// <summary>
    /// Money off the goods and carriage on top. Both default to nothing, so a plain
    /// checkout can post an empty body.
    /// </summary>
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
}
