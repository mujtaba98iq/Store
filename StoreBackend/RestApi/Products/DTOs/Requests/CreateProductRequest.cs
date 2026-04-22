namespace RestApi.Products;

public class CreateProductRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal? Price { get; set; }
    public int? Quantity { get; set; }
    public required string ImagePath { get; set; }
}
