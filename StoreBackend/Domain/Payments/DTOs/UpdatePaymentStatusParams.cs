namespace Domain.Payments;

public class UpdatePaymentStatusParams
{
    public required Guid PaymentId { get; set; }
    public required PaymentStatus PaymentStatus { get; set; }

    /// <summary>
    /// The provider's reference for the money that moved. Required when settling anything
    /// but cash on delivery, which has no provider to quote one.
    /// </summary>
    public string? TransactionId { get; set; }

    public required string UpdatedById { get; set; }
}
