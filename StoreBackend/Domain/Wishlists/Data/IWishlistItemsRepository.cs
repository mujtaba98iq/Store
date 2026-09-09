namespace Domain.Wishlists;

public interface IWishlistItemsRepository
{
    Task<WishlistItem> Create(WishlistItem wishlistItem);

    /// <summary>
    /// Nothing on an entry is editable, so in practice this carries the soft delete: it is
    /// how an entry is taken off a list, and how a list is emptied one entry at a time.
    /// </summary>
    Task<WishlistItem> Update(WishlistItem wishlistItem);

    /// <summary>
    /// Saves every entry in one round trip, so emptying a wishlist cannot leave it half
    /// cleared.
    /// </summary>
    Task<List<WishlistItem>> UpdateMany(List<WishlistItem> wishlistItems);

    Task<WishlistItem?> FindById(Guid id);

    /// <summary>
    /// Answers both "is this product already on the list" before an add and "is the heart
    /// filled" on a product page, the two being the same question.
    /// </summary>
    Task<WishlistItem?> FindByUserAndProduct(Guid userId, Guid productId);

    /// <summary>
    /// The whole of one customer's list, unpaginated, for the operations that have to touch
    /// all of it at once.
    /// </summary>
    Task<List<WishlistItem>> FindByUserId(Guid userId);

    Task<List<WishlistItem>> FindByFilters(WishlistItemFilters wishlistItemFilters);
    Task<int> GetTotalCountByFilters(WishlistItemFilters wishlistItemFilters);
}
