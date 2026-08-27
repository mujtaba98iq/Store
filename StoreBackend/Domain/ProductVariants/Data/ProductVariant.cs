using Domain.Data;
using Domain.Products;

namespace Domain.ProductVariants;

public class ProductVariant : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Product? Product { get; set; }
}
