using Sheard.Type;

namespace Domain.ProductVariants;

public class ProductVariantFilters : ListingOptions
{
    public Guid? ProductVariantId { get; set; }
    public Guid? ProductId { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
    public ProductVariantOrderBy? OrderBy { get; set; } = ProductVariantOrderBy.CreatedAt;
}
