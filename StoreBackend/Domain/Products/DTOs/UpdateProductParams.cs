namespace Domain.Products;

public class UpdateProductParams
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public string? ImagePath { get; set; }
    public string UpdatedById { get; set; }
}
