namespace Domain.ProductImages;

public class CreateProductImageParams
{
    public required Guid ProductId { get; set; }

    /// <summary>
    /// Content of the image to upload. The resulting URL is what ends up in
    /// <see cref="ProductImage.ImageUrl"/>, the caller never supplies a URL.
    /// </summary>
    public required Stream ImageContent { get; set; }

    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
    public required string CreatedById { get; set; }
}
