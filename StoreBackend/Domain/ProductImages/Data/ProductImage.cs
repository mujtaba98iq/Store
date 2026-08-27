using Domain.Data;
using Domain.Products;

namespace Domain.ProductImages;

public class ProductImage : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required string ImageUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Product? Product { get; set; }
}
