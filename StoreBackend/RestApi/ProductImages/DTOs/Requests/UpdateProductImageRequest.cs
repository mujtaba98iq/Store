namespace RestApi.ProductImages;

public class UpdateProductImageRequest
{
    public string? ImageUrl { get; set; }
    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
}
