namespace Domain.Products;

public class CreateProductParams
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string CreatedById { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public required string ImagePath { get; set; }
}
