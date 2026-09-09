using Sheard.Type;

namespace Domain.Wishlists;

public interface IWishlistService
{
    /// <summary>
    /// Puts a product on a customer's list. Adding one that is already there is not an
    /// error: the entry they already hold is returned untouched, so a heart that gets
    /// pressed twice ends up where the customer meant it to.
    /// </summary>
    Task<WishlistItem> Add(AddWishlistItemParams addWishlistItemParams);

    Task<WishlistItem?> FindById(Guid id);

    /// <summary>
    /// One customer's list, newest first, paginated.
    /// </summary>
    Task<PaginationResult<WishlistItem>> Search(WishlistItemFilters wishlistItemFilters);

    /// <summary>
    /// Whether a product is on a customer's list, which is what a product page needs to know
    /// to draw the heart one way or the other. Null rather than an error when it is not.
    /// </summary>
    Task<WishlistItem?> FindByUserAndProduct(Guid userId, Guid productId);

    /// <summary>
    /// Takes an entry off the list by its own id. An entry on somebody else's list reads as
    /// missing.
    /// </summary>
    Task<WishlistItem> Remove(RemoveWishlistItemParams removeWishlistItemParams);

    /// <summary>
    /// Takes a product off the list without the caller having to know the entry's id. What
    /// the heart on a product page calls when it is pressed a second time.
    /// </summary>
    Task<WishlistItem> RemoveProduct(RemoveWishlistProductParams removeWishlistProductParams);

    /// <summary>
    /// Empties the list. Returns how many entries came off, an already empty list being no
    /// error and simply nothing to do.
    /// </summary>
    Task<int> Clear(ClearWishlistParams clearWishlistParams);
}
