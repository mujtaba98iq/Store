using Domain.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Carts.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController(
        ICartService cartService,
        ICartResponseFormatter responseFormatter,
        IAuthorizationService authorizationService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(CartListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] CartFilters cartFilters)
        {
            var carts = await cartService.Search(cartFilters);
            return Ok(responseFormatter.Many(carts.Data, carts.TotalCount));
        }

        /// <summary>
        /// The cart of the caller. It is created on first use, so a customer who has never
        /// shopped still gets an empty cart back rather than a 404.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        public async Task<IActionResult> GetMine()
        {
            var cart = await cartService.GetOrCreateByUserId(new GetOrCreateCartParams
            {
                UserId = this.GetUserGuid(),
                CreatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(cart));
        }

        [HttpPost("me/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(AddCartItemRequestValidator))]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            var cart = await cartService.AddItem(new AddCartItemParams
            {
                UserId = this.GetUserGuid(),
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                CreatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(cart));
        }

        /// <summary>
        /// Sets the quantity of a line rather than adding to it, so a client that retries
        /// the same call cannot double what the customer asked for.
        /// </summary>
        [HttpPatch("me/items/{cartItemId:guid}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateCartItemRequestValidator))]
        public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            var cart = await cartService.UpdateItem(new UpdateCartItemParams
            {
                UserId = this.GetUserGuid(),
                CartItemId = cartItemId,
                Quantity = request.Quantity,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(cart));
        }

        [HttpDelete("me/items/{cartItemId:guid}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> RemoveItem(Guid cartItemId)
        {
            var cart = await cartService.RemoveItem(new RemoveCartItemParams
            {
                UserId = this.GetUserGuid(),
                CartItemId = cartItemId,
                DeletedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(cart));
        }

        [HttpDelete("me/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> Clear()
        {
            var cart = await cartService.Clear(new ClearCartParams
            {
                UserId = this.GetUserGuid(),
                DeletedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(cart));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cart = await cartService.FindById(id);
            if (cart is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, cart.UserId, "UserOwnerOrAdminPolicy");

            return authResult.Succeeded
                ? Ok(responseFormatter.One(cart))
                : Forbid();
        }
    }
}
