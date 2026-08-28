namespace RestApi.ProductImages;

public class UpdateProductImageRequest
{
    /// <summary>
    /// Replacement image, or omitted to keep the current one.
    /// </summary>
    public IFormFile? Image { get; set; }

    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
}
