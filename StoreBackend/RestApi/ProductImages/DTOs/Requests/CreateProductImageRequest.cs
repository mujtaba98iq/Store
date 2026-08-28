namespace RestApi.ProductImages;

public class CreateProductImageRequest
{
    public required Guid ProductId { get; set; }

    /// <summary>
    /// The image file itself. The stored URL is produced by the image storage
    /// service, the client never supplies one.
    /// </summary>
    public required IFormFile Image { get; set; }

    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
}
