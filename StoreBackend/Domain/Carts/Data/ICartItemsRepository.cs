namespace Domain.Carts;

public interface ICartItemsRepository
{
    Task<CartItem> Create(CartItem cartItem);
    Task<CartItem> Update(CartItem cartItem);
    Task<CartItem?> FindById(Guid id);
    Task<CartItem?> FindByCartIdAndProductVariantId(Guid cartId, Guid productVariantId);
    Task<List<CartItem>> FindByCartId(Guid cartId);

    /// <summary>
    /// Saves every line in one round trip, so emptying a cart cannot leave it half cleared.
    /// </summary>
    Task<List<CartItem>> UpdateMany(List<CartItem> cartItems);
}
