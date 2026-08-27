namespace Domain.ProductImages;

public class UpdateProductImageParams
{
    public required Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
    public required string UpdatedById { get; set; }
}
