using Domain.Payments;

namespace RestApi.Payments;

public class UpdatePaymentStatusRequest
{
    public required PaymentStatus PaymentStatus { get; set; }

    /// <summary>
    /// The provider's reference for the money that moved. Required when settling anything
    /// but cash on delivery.
    /// </summary>
    public string? TransactionId { get; set; }
}
