namespace RestApi.Carts;

public class CartItemResponse
{
    public required string Id { get; set; }
    public required string CartId { get; set; }
    public required string ProductVariantId { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }

    /// <summary>
    /// Quantity * UnitPrice. Exposed so clients never have to compute it themselves.
    /// </summary>
    public required decimal Subtotal { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
