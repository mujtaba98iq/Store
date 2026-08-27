namespace RestApi.ProductVariants;

public class ProductVariantResponse
{
    public required string Id { get; set; }
    public required string ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
