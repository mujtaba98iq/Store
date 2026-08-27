namespace RestApi.ProductImages;

public class CreateProductImageRequest
{
    public required Guid ProductId { get; set; }
    public required string ImageUrl { get; set; }
    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
}
