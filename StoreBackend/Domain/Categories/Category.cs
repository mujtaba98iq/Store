using Domain.Data;
using Domain.Products;

namespace Domain.Categories;

public class Category : IAuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }
}
