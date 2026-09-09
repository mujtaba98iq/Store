using Domain.Wishlists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Wishlists.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController(
        IWishlistService wishlistService,
        IWishlistItemResponseFormatter responseFormatter,
        IAuthorizationService authorizationService) : ControllerBase
    {
        /// <summary>
        /// Every entry on every list. Staff only, a wishlist being nobody's business but its
        /// owner's. Filtered by product it answers how many people are waiting on something,
        /// which is what makes a restock worth ordering.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(WishlistItemListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] WishlistItemFilters wishlistItemFilters)
        {
            var wishlistItems = await wishlistService.Search(wishlistItemFilters);
            return Ok(responseFormatter.Many(wishlistItems.Data, wishlistItems.TotalCount));
        }

        /// <summary>
        /// The caller's own list, newest first. The read every customer makes, and the only
        /// route that will ever show them their own wishlist.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(WishlistItemListResponse), 200)]
        public async Task<IActionResult> GetMine([FromQuery] WishlistItemFilters wishlistItemFilters)
        {
            // Overwritten rather than read from the query, so this route can only ever return
            // the caller's own entries, however the filter arrived.
            wishlistItemFilters.UserId = this.GetUserGuid();

            var wishlistItems = await wishlistService.Search(wishlistItemFilters);
            return Ok(responseFormatter.Many(wishlistItems.Data, wishlistItems.TotalCount));
        }

        /// <summary>
        /// Whether one product is on the caller's list. What a product page asks to decide
        /// which way round to draw the heart, without having to pull the whole list down to
        /// find out.
        /// </summary>
        [HttpGet("me/products/{productId:guid}")]
        [ProducesResponseType(typeof(WishlistItemResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetMineByProductId(Guid productId)
        {
            var wishlistItem = await wishlistService.FindByUserAndProduct(this.GetUserGuid(), productId);

            return wishlistItem is null
                ? NotFound()
                : Ok(responseFormatter.One(wishlistItem));
        }

        /// <summary>
        /// Puts a product on the caller's list.
        ///
        /// Answers 200 rather than 201 because the same request twice leaves the same one
        /// entry: pressing a heart that is already filled is not a second wish, and is not an
        /// error either, so the entry that already stands is what comes back.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(WishlistItemResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(AddWishlistItemRequestValidator))]
        public async Task<IActionResult> Add([FromBody] AddWishlistItemRequest request)
        {
            var wishlistItem = await wishlistService.Add(new AddWishlistItemParams
            {
                UserId = this.GetUserGuid(),
                ProductId = request.ProductId,
                CreatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(wishlistItem));
        }

        /// <summary>
        /// Takes an entry off the caller's list, addressed by the entry's own id. What a
        /// wishlist page uses, having the entries in front of it. An entry belonging to
        /// anybody else reads as missing.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> Remove(Guid id)
        {
            await wishlistService.Remove(new RemoveWishlistItemParams
            {
                WishlistItemId = id,
                UserId = this.GetUserGuid(),
                DeletedById = this.GetUserId()
            });

            return NoContent();
        }

        /// <summary>
        /// Takes a product off the caller's list without being told which entry holds it.
        /// What a product page uses: the heart there knows the product it is showing and
        /// nothing about the row behind it.
        /// </summary>
        [HttpDelete("products/{productId:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> RemoveProduct(Guid productId)
        {
            await wishlistService.RemoveProduct(new RemoveWishlistProductParams
            {
                UserId = this.GetUserGuid(),
                ProductId = productId,
                DeletedById = this.GetUserId()
            });

            return NoContent();
        }

        /// <summary>
        /// Empties the caller's list in one go.
        /// </summary>
        [HttpDelete("me")]
        [ProducesResponseType(typeof(WishlistClearedResponse), 200)]
        public async Task<IActionResult> Clear()
        {
            var removedCount = await wishlistService.Clear(new ClearWishlistParams
            {
                UserId = this.GetUserGuid(),
                DeletedById = this.GetUserId()
            });

            return Ok(new WishlistClearedResponse { RemovedCount = removedCount });
        }

        /// <summary>
        /// One entry, for the owner or for staff. Reported as missing to anybody else rather
        /// than refused, because what a stranger has on their wishlist is not something to be
        /// learned by guessing at ids.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(WishlistItemResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var wishlistItem = await wishlistService.FindById(id);

            if (wishlistItem is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, wishlistItem.UserId, "UserOwnerOrAdminPolicy");

            return authResult.Succeeded
                ? Ok(responseFormatter.One(wishlistItem))
                : NotFound();
        }
    }
}
