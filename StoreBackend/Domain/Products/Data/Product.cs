using Domain.Data;

namespace Domain.Products;

public class Product : IAuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public required string ImagePath { get; set; }

    public DateTime CreatedAt  { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById  { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }
}
