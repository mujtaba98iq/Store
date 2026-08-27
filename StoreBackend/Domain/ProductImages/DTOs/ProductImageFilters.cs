using Sheard.Type;

namespace Domain.ProductImages;

public class ProductImageFilters : ListingOptions
{
    public Guid? ProductImageId { get; set; }
    public Guid? ProductId { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
    public ProductImageOrderBy? OrderBy { get; set; } = ProductImageOrderBy.DisplayOrder;
}
