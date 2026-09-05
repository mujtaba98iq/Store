using Domain.Orders;
using Domain.Shipments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Shipments.Controllers
{
    /// <summary>
    /// Mostly warehouse work: opening a parcel, naming its carrier and moving it along are
    /// all staff-only, and marked so one action at a time. Customers get the reads, and then
    /// only of their own parcels — which is checked against the order, not the role.
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentsController(
        IShipmentService shipmentService,
        IOrderService orderService,
        IShipmentResponseFormatter responseFormatter,
        IAuthorizationService authorizationService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ShipmentListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] ShipmentFilters shipmentFilters)
        {
            var shipments = await shipmentService.Search(shipmentFilters);
            return Ok(responseFormatter.Many(shipments.Data, shipments.TotalCount));
        }

        /// <summary>
        /// The parcels for the caller's own orders.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ShipmentListResponse), 200)]
        public async Task<IActionResult> GetMine([FromQuery] ShipmentFilters shipmentFilters)
        {
            // Overwritten rather than read from the query, so this route can only ever return
            // the caller's own parcels however the filter arrived.
            shipmentFilters.UserId = this.GetUserGuid();

            var shipments = await shipmentService.Search(shipmentFilters);
            return Ok(responseFormatter.Many(shipments.Data, shipments.TotalCount));
        }

        /// <summary>
        /// The parcel for one order. There is at most one.
        /// </summary>
        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(ShipmentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            // Authorised against the order rather than against the parcel, so an order that
            // has none yet answers the same way as one that has.
            var order = await orderService.FindById(orderId);
            if (order is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, order.UserId, "UserOwnerOrAdminPolicy");
            if (!authResult.Succeeded)
            {
                return Forbid();
            }

            var shipment = await shipmentService.FindByOrderId(orderId);

            return shipment is null
                ? NotFound()
                : Ok(responseFormatter.One(shipment));
        }

        /// <summary>
        /// Opens a parcel for an order that has none. Checkout does this for every order
        /// placed since shipments existed, so this is for the ones placed before that.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ShipmentResponse), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(CreateShipmentRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
        {
            var shipment = await shipmentService.Create(new CreateShipmentParams
            {
                OrderId = request.OrderId,
                TrackingNumber = request.TrackingNumber,
                ShippingProvider = request.ShippingProvider,
                CreatedById = this.GetUserId()
            });

            return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, responseFormatter.One(shipment));
        }

        /// <summary>
        /// Records who is carrying the parcel and under what number. Anything left out keeps
        /// the value it had.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(ShipmentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateShipmentRequestValidator))]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentRequest request)
        {
            var shipment = await shipmentService.Update(new UpdateShipmentParams
            {
                ShipmentId = id,
                TrackingNumber = request.TrackingNumber,
                ShippingProvider = request.ShippingProvider,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(shipment));
        }

        /// <summary>
        /// Moves the parcel along its own lifecycle. The order has a status of its own and
        /// does not follow this one: an order is marked shipped and delivered in its own
        /// right, and doing so brings the parcel with it.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ShipmentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateShipmentStatusRequestValidator))]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request)
        {
            var shipment = await shipmentService.UpdateStatus(new UpdateShipmentStatusParams
            {
                ShipmentId = id,
                Status = request.Status,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(shipment));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ShipmentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var shipment = await shipmentService.FindById(id);

            // The order is what says who the parcel belongs to, so one that came back without
            // an order cannot be shown to anybody.
            if (shipment?.Order is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, shipment.Order.UserId, "UserOwnerOrAdminPolicy");

            return authResult.Succeeded
                ? Ok(responseFormatter.One(shipment))
                : Forbid();
        }
    }
}
