using Domain.Orders;
using Domain.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Payments.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(
        IPaymentService paymentService,
        IOrderService orderService,
        IPaymentResponseFormatter responseFormatter,
        IAuthorizationService authorizationService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PaymentListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] PaymentFilters paymentFilters)
        {
            var payments = await paymentService.Search(paymentFilters);
            return Ok(responseFormatter.Many(payments.Data, payments.TotalCount));
        }

        /// <summary>
        /// Everything the caller has ever been asked to pay.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(PaymentListResponse), 200)]
        public async Task<IActionResult> GetMine([FromQuery] PaymentFilters paymentFilters)
        {
            // Overwritten rather than read from the query, so this route can only ever return
            // the caller's own payments however the filter arrived.
            paymentFilters.UserId = this.GetUserGuid();

            var payments = await paymentService.Search(paymentFilters);
            return Ok(responseFormatter.Many(payments.Data, payments.TotalCount));
        }

        /// <summary>
        /// Every attempt against one order, oldest first: the decline and the retry that
        /// followed it both show.
        /// </summary>
        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(List<PaymentResponse>), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            // Authorised against the order rather than against the payments, so an order with
            // nothing recorded yet answers the same way as one with a full history.
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

            var payments = await paymentService.FindByOrderId(orderId);

            return Ok(payments.Select(responseFormatter.One).ToList());
        }

        /// <summary>
        /// Opens a fresh attempt against an order, for whatever it came to. Used when the
        /// last one was declined, or when the customer wants to pay a different way.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponse), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(RecordPaymentRequestValidator))]
        public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request)
        {
            var order = await orderService.FindById(request.OrderId);
            if (order is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, order.UserId, "UserOwnerOrAdminPolicy");
            if (!authResult.Succeeded)
            {
                return Forbid();
            }

            var payment = await paymentService.Record(new RecordPaymentParams
            {
                OrderId = request.OrderId,
                PaymentMethod = request.PaymentMethod,
                CreatedById = this.GetUserId()
            });

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, responseFormatter.One(payment));
        }

        /// <summary>
        /// Settles, fails or refunds an attempt. Staff only: this is where a provider's
        /// answer is written down, and moving money is not something a customer does to
        /// their own record.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(PaymentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdatePaymentStatusRequestValidator))]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePaymentStatusRequest request)
        {
            var payment = await paymentService.UpdateStatus(new UpdatePaymentStatusParams
            {
                PaymentId = id,
                PaymentStatus = request.PaymentStatus,
                TransactionId = request.TransactionId,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(payment));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PaymentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await paymentService.FindById(id);

            // The order is what says who the payment belongs to, so a payment that came back
            // without one cannot be shown to anybody.
            if (payment?.Order is null)
            {
                return NotFound();
            }

            var authResult = await authorizationService.AuthorizeAsync(User, payment.Order.UserId, "UserOwnerOrAdminPolicy");

            return authResult.Succeeded
                ? Ok(responseFormatter.One(payment))
                : Forbid();
        }
    }
}
