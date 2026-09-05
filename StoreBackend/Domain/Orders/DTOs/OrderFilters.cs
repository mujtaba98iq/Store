using Sheard.Type;

namespace Domain.Orders;

public class OrderFilters : ListingOptions
{
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public string? OrderNumber { get; set; }
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Bounds on when the order was placed. Inclusive at both ends.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }

    public OrderOrderBy? OrderBy { get; set; } = OrderOrderBy.CreatedAt;
}
