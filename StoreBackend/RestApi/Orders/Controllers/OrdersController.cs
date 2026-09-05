using Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Orders.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(
        IOrderService orderService,
        IOrderResponseFormatter responseFormatter,
        IAuthorizationService authorizationService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(OrderListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] OrderFilters orderFilters)
        {
            var orders = await orderService.Search(orderFilters);
            return Ok(responseFormatter.Many(orders.Data, orders.TotalCount));
        }

        /// <summary>
        /// The order history of the caller.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(OrderListResponse), 200)]
        public async Task<IActionResult> GetMine([FromQuery] OrderFilters orderFilters)
        {
            // Overwritten rather than read from the query, so this route can only ever return
            // the caller's own orders however the filter arrived.
            orderFilters.UserId = this.GetUserGuid();

            var orders = await orderService.Search(orderFilters);
            return Ok(responseFormatter.Many(orders.Data, orders.TotalCount));
        }

        /// <summary>
        /// Places the contents of the caller's cart as an order and empties the cart.
        /// </summary>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(CheckoutRequestValidator))]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var order = await orderService.Checkout(new CheckoutParams
            {
                UserId = this.GetUserGuid(),
                DiscountAmount = request.DiscountAmount,
                ShippingAmount = request.ShippingAmount,
                CreatedById = this.GetUserId()
            });

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, responseFormatter.One(order));
        }

        /// <summary>
        /// Calls off the caller's own order and releases the stock it was holding. Only
        /// possible up to the point it ships.
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var order = await orderService.Cancel(new CancelOrderParams
            {
                OrderId = id,
                UserId = this.GetUserGuid(),
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(order));
        }

        /// <summary>
        /// Moves an order along its lifecycle. Staff only, because shipping is what turns a
        /// reservation into a real deduction from stock.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateOrderStatusRequestValidator))]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await orderService.UpdateStatus(new UpdateOrderStatusParams
            {
                OrderId = id,
                Status = request.Status,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(order));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await orderService.FindById(id);
            if (order is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, order.UserId, "UserOwnerOrAdminPolicy");

            return authResult.Succeeded
                ? Ok(responseFormatter.One(order))
                : Forbid();
        }
    }
}
