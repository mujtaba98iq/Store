using Domain.Payments;

namespace Domain.Orders;

public class CheckoutParams
{
    public required Guid UserId { get; set; }

    /// <summary>
    /// The coupon the customer is quoting, if any. A code rather than an amount: what comes
    /// off is worked out here from the campaign's own terms, so a client cannot name its own
    /// discount. Null or blank checks out at full price.
    ///
    /// Redeeming it is part of placing the order, which is why the code is taken here rather
    /// than applied afterwards: the usage limit has to be spent in the same breath as the
    /// order that spent it.
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Carriage on top of the goods, still supplied by the caller: shipping rates are not
    /// worked out anywhere yet.
    /// </summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Where the order is going. Taken with the checkout and copied onto the order, so it
    /// still reads correctly after the customer moves.
    /// </summary>
    public required CheckoutShippingAddress ShippingAddress { get; set; }

    /// <summary>
    /// How the customer intends to pay. Required, because an order is opened with a payment
    /// against it and a payment has to say by what means it is being made. It is a statement
    /// of intent rather than of fact: nothing has been settled at this point.
    /// </summary>
    public required PaymentMethod PaymentMethod { get; set; }

    public required string CreatedById { get; set; }
}
