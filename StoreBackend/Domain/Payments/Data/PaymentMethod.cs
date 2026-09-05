namespace Domain.Payments;

/// <summary>
/// How a payment is being made. The values are persisted as numbers, so they may be added
/// to but never renumbered.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Collected by the courier on handover. Nothing is taken at checkout, so a payment on
    /// this method stays pending until the parcel actually arrives.
    /// </summary>
    CashOnDelivery = 1,

    Card = 2,
    ZainCash = 3,
    FastPay = 4,

    /// <summary>
    /// Settled by the customer at their own bank, outside the shop. It is confirmed by hand
    /// once the funds are seen, rather than by a provider calling back.
    /// </summary>
    BankTransfer = 5,
}
