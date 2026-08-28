namespace Domain.ProductImages;

public class UpdateProductImageParams
{
    public required Guid Id { get; set; }

    /// <summary>
    /// Content of the replacement image, or null to keep the current one.
    /// </summary>
    public Stream? ImageContent { get; set; }

    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
    public required string UpdatedById { get; set; }
}
