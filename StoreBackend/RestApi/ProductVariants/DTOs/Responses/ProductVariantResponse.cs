namespace RestApi.ProductVariants;

public class ProductVariantResponse
{
    public required string Id { get; set; }
    public required string ProductId { get; set; }

    /// <summary>
    /// The name of the product this variant belongs to, carried here so a listing does
    /// not have to fetch every product to label its rows. Null only if the product
    /// behind it is gone.
    /// </summary>
    public string? ProductName { get; set; }
    public required string Sku { get; set; }
    public decimal? Price { get; set; }
    public string? Barcode { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
