using Domain.Wishlists;

namespace RestApi.Wishlists;

public class WishlistItemResponseFormatter : IWishlistItemResponseFormatter
{
    public WishlistItemListResponse Many(IEnumerable<WishlistItem> wishlistItems, int totalCount)
    {
        return new WishlistItemListResponse
        {
            Data = wishlistItems.Select(One).ToList(),
            TotalCount = totalCount
        };
    }

    public WishlistItemResponse One(WishlistItem wishlistItem)
    {
        return new WishlistItemResponse
        {
            Id = wishlistItem.Id.ToString(),
            UserId = wishlistItem.UserId.ToString(),
            ProductId = wishlistItem.ProductId.ToString(),
            CreatedAt = wishlistItem.CreatedAt,
            CreatedById = wishlistItem.CreatedById
        };
    }
}
