namespace Domain.ProductVariants;

public class CreateProductVariantParams
{
    public required Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public bool? IsActive { get; set; }
    public required string CreatedById { get; set; }
}
