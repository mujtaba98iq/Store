namespace RestApi.Orders;

public class CheckoutRequest
{
    /// <summary>
    /// Money off the goods and carriage on top. Both default to nothing.
    /// </summary>
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Where the order is going. Required: an order with nowhere to send it cannot be
    /// fulfilled, and the address given here is the one the order keeps.
    /// </summary>
    public required ShippingAddressRequest ShippingAddress { get; set; }
}
