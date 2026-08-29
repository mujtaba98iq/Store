using Domain.Carts;

namespace RestApi.Carts;

public class CartResponseFormatter : ICartResponseFormatter
{
    public CartListResponse Many(IEnumerable<Cart> carts, int totalCount)
    {
        var cartResults = carts.Select(One).ToList();

        return new CartListResponse
        {
            Data = cartResults,
            TotalCount = totalCount
        };
    }

    public CartResponse One(Cart cart)
    {
        // Repositories load the live lines only, but a cart handed over straight after a
        // removal still carries the line that was just taken out.
        var items = cart.Items
            .Where(i => i.DeletedAt == null)
            .OrderBy(i => i.CreatedAt)
            .Select(One)
            .ToList();

        return new CartResponse
        {
            Id = cart.Id.ToString(),
            UserId = cart.UserId.ToString(),
            Items = items,
            ItemCount = items.Count,
            TotalAmount = items.Sum(i => i.Subtotal),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            CreatedById = cart.CreatedById,
            UpdatedById = cart.UpdatedById
        };
    }

    private static CartItemResponse One(CartItem cartItem)
    {
        return new CartItemResponse
        {
            Id = cartItem.Id.ToString(),
            CartId = cartItem.CartId.ToString(),
            ProductVariantId = cartItem.ProductVariantId.ToString(),
            Quantity = cartItem.Quantity,
            UnitPrice = cartItem.UnitPrice,
            Subtotal = cartItem.Subtotal,
            CreatedAt = cartItem.CreatedAt,
            UpdatedAt = cartItem.UpdatedAt
        };
    }
}
