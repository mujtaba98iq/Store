namespace Domain.Payments;

public class RecordPaymentParams
{
    public required Guid OrderId { get; set; }

    /// <summary>
    /// How this attempt is being made. It may differ from the last one: a customer whose
    /// card was declined can come back with cash on delivery.
    /// </summary>
    public required PaymentMethod PaymentMethod { get; set; }

    public required string CreatedById { get; set; }
}
