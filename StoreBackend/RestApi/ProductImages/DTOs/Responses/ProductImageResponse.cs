namespace RestApi.ProductImages;

public class ProductImageResponse
{
    public required string Id { get; set; }
    public required string ProductId { get; set; }
    public required string ImageUrl { get; set; }
    public required bool IsPrimary { get; set; }
    public required int DisplayOrder { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
