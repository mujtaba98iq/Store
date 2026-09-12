namespace Domain.ProductVariants;

public class DeleteProductVariantParams
{
    public required Guid Id { get; set; }
    public required string DeletedById { get; set; }
}
