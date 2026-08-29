namespace RestApi.Carts;

public class CartResponse
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required List<CartItemResponse> Items { get; set; }

    /// <summary>
    /// Number of lines in the cart, not the number of units across them.
    /// </summary>
    public required int ItemCount { get; set; }

    /// <summary>
    /// Sum of every line subtotal.
    /// </summary>
    public required decimal TotalAmount { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
}
