using Domain.Data;
using Domain.ProductVariants;

namespace Domain.Inventories;

public class Inventory : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public ProductVariant? ProductVariant { get; set; }
}
