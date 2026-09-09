using Domain.Products;
using Sheard.Type;

namespace Domain.Wishlists
{
    public class WishlistService(
        IWishlistItemsRepository wishlistItemsRepository,
        IProductsRepository productsRepository) : IWishlistService
    {
        public async Task<WishlistItem> Add(AddWishlistItemParams addWishlistItemParams)
        {
            // The product has to exist, but nothing more is asked of it. Whether it is in
            // stock, active or even buyable is beside the point: waiting for something to
            // come back is one of the things a wishlist is for. Those questions get asked at
            // the cart, where the customer actually intends to pay.
            _ = await productsRepository.FindById(addWishlistItemParams.ProductId)
                ?? throw new ResourceNotFoundException("Product", $"Product with ID {addWishlistItemParams.ProductId} not found");

            // Adding something already on the list is answered with the entry that is there
            // rather than refused. A wishlist entry carries nothing to merge — no quantity to
            // top up, as a cart line would have — so a second add is a request for a state
            // that already holds, and the honest answer is the row that already holds it.
            var existingItem = await wishlistItemsRepository.FindByUserAndProduct(
                addWishlistItemParams.UserId,
                addWishlistItemParams.ProductId);

            if (existingItem != null)
            {
                return existingItem;
            }

            return await wishlistItemsRepository.Create(new WishlistItem
            {
                Id = Guid.NewGuid(),
                UserId = addWishlistItemParams.UserId,
                ProductId = addWishlistItemParams.ProductId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = addWishlistItemParams.CreatedById
            });
        }

        public async Task<WishlistItem?> FindById(Guid id)
        {
            return await wishlistItemsRepository.FindById(id);
        }

        public async Task<WishlistItem?> FindByUserAndProduct(Guid userId, Guid productId)
        {
            return await wishlistItemsRepository.FindByUserAndProduct(userId, productId);
        }

        public async Task<PaginationResult<WishlistItem>> Search(WishlistItemFilters wishlistItemFilters)
        {
            var wishlistItems = await wishlistItemsRepository.FindByFilters(wishlistItemFilters);
            var totalCount = await wishlistItemsRepository.GetTotalCountByFilters(wishlistItemFilters);

            return new PaginationResult<WishlistItem>
            {
                TotalCount = totalCount,
                Data = wishlistItems
            };
        }

        public async Task<WishlistItem> Remove(RemoveWishlistItemParams removeWishlistItemParams)
        {
            var wishlistItem = await wishlistItemsRepository.FindById(removeWishlistItemParams.WishlistItemId);

            // An entry on somebody else's list is reported as missing rather than refused, as
            // a review is: whether a stranger has a given product on their wishlist is not
            // something to be learned by guessing at ids.
            if (wishlistItem == null || wishlistItem.UserId != removeWishlistItemParams.UserId)
            {
                throw new ResourceNotFoundException("WishlistItem", $"Wishlist item with ID {removeWishlistItemParams.WishlistItemId} not found");
            }

            return await SoftDelete(wishlistItem, removeWishlistItemParams.DeletedById);
        }

        public async Task<WishlistItem> RemoveProduct(RemoveWishlistProductParams removeWishlistProductParams)
        {
            var wishlistItem = await wishlistItemsRepository.FindByUserAndProduct(
                                   removeWishlistProductParams.UserId,
                                   removeWishlistProductParams.ProductId)
                               ?? throw new ResourceNotFoundException("WishlistItem", $"Product with ID {removeWishlistProductParams.ProductId} is not on this wishlist");

            return await SoftDelete(wishlistItem, removeWishlistProductParams.DeletedById);
        }

        public async Task<int> Clear(ClearWishlistParams clearWishlistParams)
        {
            var wishlistItems = await wishlistItemsRepository.FindByUserId(clearWishlistParams.UserId);

            if (wishlistItems.Count == 0)
            {
                return 0;
            }

            var deletedAt = DateTime.UtcNow;
            foreach (var wishlistItem in wishlistItems)
            {
                wishlistItem.DeletedAt = deletedAt;
                wishlistItem.DeletedById = clearWishlistParams.DeletedById;
            }

            await wishlistItemsRepository.UpdateMany(wishlistItems);

            return wishlistItems.Count;
        }

        /// <summary>
        /// Entries are struck out rather than dropped, as cart lines are, so what a customer
        /// wanted and thought better of is still answerable afterwards. The unique index that
        /// keeps one entry per product is filtered on the same column, so a product taken off
        /// a list can be put back on it.
        /// </summary>
        private async Task<WishlistItem> SoftDelete(WishlistItem wishlistItem, string deletedById)
        {
            wishlistItem.DeletedAt = DateTime.UtcNow;
            wishlistItem.DeletedById = deletedById;

            await wishlistItemsRepository.Update(wishlistItem);

            return wishlistItem;
        }
    }
}
