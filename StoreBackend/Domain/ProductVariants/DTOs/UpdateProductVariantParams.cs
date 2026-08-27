namespace Domain.ProductVariants;

public class UpdateProductVariantParams
{
    public required Guid Id { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public bool? IsActive { get; set; }
    public required string UpdatedById { get; set; }
}
