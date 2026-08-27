namespace Domain.ProductImages;

public class CreateProductImageParams
{
    public required Guid ProductId { get; set; }
    public required string ImageUrl { get; set; }
    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
    public required string CreatedById { get; set; }
}
