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

    public required string CreatedById { get; set; }
}
