using Domain.Payments;

namespace RestApi.Orders;

public class CheckoutRequest
{
    /// <summary>
    /// The coupon the customer is quoting. A code, not an amount: what comes off is worked
    /// out on the server from the campaign's own terms, so a client cannot name its own
    /// discount. Leave it out or send it empty to check out at full price.
    ///
    /// A code that cannot be used stops the checkout rather than being ignored. The customer
    /// meant to buy at a discount, and billing them full price instead is not the shop's
    /// decision — preview the code first if the basket page needs to know.
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Carriage on top of the goods. Defaults to nothing.
    /// </summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Where the order is going. Required: an order with nowhere to send it cannot be
    /// fulfilled, and the address given here is the one the order keeps.
    /// </summary>
    public required ShippingAddressRequest ShippingAddress { get; set; }

    /// <summary>
    /// How the customer means to pay. Required: checkout opens a payment against the order,
    /// and a payment has to say by what means. Nothing is taken here — the payment starts
    /// out pending whichever method is chosen.
    /// </summary>
    public required PaymentMethod PaymentMethod { get; set; }
}
