namespace Domain.Orders;

public class CancelOrderParams
{
    public required Guid OrderId { get; set; }

    /// <summary>
    /// The customer doing the cancelling. Checked against the order so one customer can
    /// never call off another's.
    /// </summary>
    public required Guid UserId { get; set; }

    public required string UpdatedById { get; set; }
}
