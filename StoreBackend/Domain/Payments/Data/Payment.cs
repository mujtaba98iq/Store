using Domain.Data;
using Domain.Orders;

namespace Domain.Payments;

/// <summary>
/// One attempt to move money for an order. An order can hold several of these, because
/// paying is rarely a single event: a card is declined and then retried, or a settled
/// payment is refunded weeks later. Keeping every attempt is what lets an order say not
/// only that it was paid, but how it got there.
/// </summary>
public class Payment : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>
    /// How the customer chose to pay. It decides who settles the money and when: a card is
    /// taken at checkout, while cash on delivery is collected by the courier and so sits at
    /// <see cref="PaymentStatus.Pending"/> until the parcel is handed over.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// What this attempt was for. A copy, not a look at the order's total: a part payment or
    /// a refund has to be able to say what actually moved, which is not always the whole bill.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The reference the payment provider gave back, kept so a row here can be matched
    /// against the provider's own records during a dispute. Unique where present, which is
    /// what stops the same provider transaction being banked twice if a callback arrives
    /// more than once.
    ///
    /// Null where there is nothing to quote: cash on delivery never reaches a provider, and
    /// neither does a request that failed before it got there.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// When the money actually landed. Null until the payment reaches
    /// <see cref="PaymentStatus.Paid"/>, and left alone if it is later refunded: a refund is
    /// money going back, not a correction of when it arrived.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Order? Order { get; set; }
}
