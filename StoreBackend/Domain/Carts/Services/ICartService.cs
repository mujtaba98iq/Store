using Sheard.Type;

namespace Domain.Carts;

public interface ICartService
{
    /// <summary>
    /// Returns the customer's cart, creating an empty one the first time they need it.
    /// </summary>
    Task<Cart> GetOrCreateByUserId(GetOrCreateCartParams getOrCreateCartParams);

    Task<Cart?> FindById(Guid id);
    Task<Cart?> FindByUserId(Guid userId);
    Task<PaginationResult<Cart>> Search(CartFilters cartFilters);

    Task<Cart> AddItem(AddCartItemParams addCartItemParams);
    Task<Cart> UpdateItem(UpdateCartItemParams updateCartItemParams);
    Task<Cart> RemoveItem(RemoveCartItemParams removeCartItemParams);
    Task<Cart> Clear(ClearCartParams clearCartParams);
}
