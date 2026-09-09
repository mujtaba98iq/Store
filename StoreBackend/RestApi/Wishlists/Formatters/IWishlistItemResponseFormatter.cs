using Domain.Wishlists;

namespace RestApi.Wishlists;

public interface IWishlistItemResponseFormatter
{
    WishlistItemResponse One(WishlistItem wishlistItem);
    WishlistItemListResponse Many(IEnumerable<WishlistItem> wishlistItems, int totalCount);
}
