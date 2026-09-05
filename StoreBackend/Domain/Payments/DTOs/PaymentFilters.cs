using Sheard.Type;

namespace Domain.Payments;

public class PaymentFilters : ListingOptions
{
    public Guid? PaymentId { get; set; }
    public Guid? OrderId { get; set; }

    /// <summary>
    /// The customer who placed the order this payment settles. Payments carry no user of
    /// their own, so this is read through the order.
    /// </summary>
    public Guid? UserId { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Bounds on when the attempt was recorded. Inclusive at both ends.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public PaymentOrderBy? OrderBy { get; set; } = PaymentOrderBy.CreatedAt;
}
