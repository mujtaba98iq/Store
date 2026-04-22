using Sheard.Type;

namespace Domain.Products;

public class ProductFilters : ListingOptions
{
    public string? Name { get; set; }
    public Guid? ProductId { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public ProductOrderBy? OrderBy { get; set; } = ProductOrderBy.CreatedAt;
}
