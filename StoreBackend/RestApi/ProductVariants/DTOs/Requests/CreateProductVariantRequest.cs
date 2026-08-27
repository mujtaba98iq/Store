namespace RestApi.ProductVariants;

public class CreateProductVariantRequest
{
    public required Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public bool? IsActive { get; set; }
}
