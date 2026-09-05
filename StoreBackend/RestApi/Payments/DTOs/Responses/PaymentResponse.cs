namespace RestApi.Payments;

public class PaymentResponse
{
    public required string Id { get; set; }
    public required string OrderId { get; set; }

    /// <summary>
    /// Method and status are rendered by name rather than by number, so a client never has
    /// to carry a copy of the enums to make sense of them.
    /// </summary>
    public required string PaymentMethod { get; set; }
    public required string PaymentStatus { get; set; }

    /// <summary>
    /// What this attempt was for, which is the whole of what the order came to.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// The provider's reference. Null on cash, and on anything not settled yet.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// When the money landed. Null until it has, and unchanged by a later refund.
    /// </summary>
    public DateTime? PaidAt { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
