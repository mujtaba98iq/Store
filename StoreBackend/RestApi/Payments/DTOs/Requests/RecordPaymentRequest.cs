using Domain.Payments;

namespace RestApi.Payments;

public class RecordPaymentRequest
{
    /// <summary>
    /// The order being settled. The amount is not asked for: what is owed comes off the
    /// order, and is not something a caller gets to name.
    /// </summary>
    public required Guid OrderId { get; set; }

    public required PaymentMethod PaymentMethod { get; set; }
}
