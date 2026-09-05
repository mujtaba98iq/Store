namespace Domain.Payments;

/// <summary>
/// Where a payment has got to. The values are persisted as numbers, so they may be added
/// to but never renumbered.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Recorded but not settled: awaiting the provider, or awaiting the courier in the case
    /// of cash on delivery.
    /// </summary>
    Pending = 1,

    Paid = 2,

    /// <summary>
    /// The attempt did not go through. It is kept rather than discarded, so a retry can be
    /// told apart from a first try.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Money returned to the customer. It is recorded on the payment that took the money in
    /// the first place, which is why a refund never sends a payment back to
    /// <see cref="Pending"/>.
    /// </summary>
    Refunded = 4,
}
