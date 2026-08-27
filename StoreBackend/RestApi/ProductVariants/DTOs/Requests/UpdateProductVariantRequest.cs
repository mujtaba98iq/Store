namespace RestApi.ProductVariants;

public class UpdateProductVariantRequest
{
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public bool? IsActive { get; set; }
}
