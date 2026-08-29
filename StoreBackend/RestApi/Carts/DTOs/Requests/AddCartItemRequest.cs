namespace RestApi.Carts;

public class AddCartItemRequest
{
    public required Guid ProductVariantId { get; set; }
    public required int Quantity { get; set; }
}
