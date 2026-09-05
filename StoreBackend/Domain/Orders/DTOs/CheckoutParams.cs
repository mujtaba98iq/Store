using Domain.Payments;

namespace Domain.Orders;

public class CheckoutParams
{
    public required Guid UserId { get; set; }

    /// <summary>
    /// Money taken off and added on top of the goods. Both are supplied by the caller for
    /// now; when promotions and shipping rates exist they will be worked out here instead.
    /// </summary>
    public decimal DiscountAmount { get; set; }
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
