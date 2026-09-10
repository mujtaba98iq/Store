using Domain.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Coupons.Controllers
{
    /// <summary>
    /// Staff-facing by default. A campaign's terms — how many redemptions are left, what
    /// floor an order has to clear — are not a customer's business, and a coupon table a
    /// customer could read is a coupon table they could work through code by code.
    ///
    /// The one exception is <see cref="Preview"/>, which answers about a single code the
    /// customer already holds.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController(
        ICouponService couponService,
        ICouponResponseFormatter responseFormatter) : ControllerBase
    {
        /// <summary>
        /// Every campaign, live or not. isRedeemable=true narrows it to the ones a customer
        /// could actually use this moment, which is not the same as isActive=true: an active
        /// coupon whose window has closed is not redeemable.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CouponListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] CouponFilters couponFilters)
        {
            var coupons = await couponService.Search(couponFilters);
            return Ok(responseFormatter.Many(coupons.Data, coupons.TotalCount));
        }

        /// <summary>
        /// Sets up a campaign. Refused if the terms do not hold together, or if the code is
        /// one another coupon already answers to.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CouponResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateCouponRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateCouponRequest request)
        {
            var coupon = await couponService.Create(new CreateCouponParams
            {
                Code = request.Code,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinimumOrderAmount = request.MinimumOrderAmount,
                MaximumDiscountAmount = request.MaximumDiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                IsActive = request.IsActive,
                CreatedById = this.GetUserId()
            });

            return CreatedAtAction(nameof(GetById), new { id = coupon.Id }, responseFormatter.One(coupon));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CouponResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var coupon = await couponService.FindById(id);

            return coupon is null
                ? NotFound()
                : Ok(responseFormatter.One(coupon));
        }

        /// <summary>
        /// Looks a campaign up by the code rather than the key, which is how staff have it
        /// when a customer is on the phone about it.
        /// </summary>
        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(CouponResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var coupon = await couponService.FindByCode(code);

            return coupon is null
                ? NotFound()
                : Ok(responseFormatter.One(coupon));
        }

        /// <summary>
        /// Revises a campaign's terms, or switches it off and on. The code is not editable:
        /// orders keep their own copy of what was quoted, and renaming a coupon would leave
        /// the two disagreeing.
        ///
        /// What is already spent stays spent. Changing the terms does not reach back into
        /// orders placed under the old ones.
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(CouponResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateCouponRequestValidator))]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponRequest request)
        {
            var coupon = await couponService.Update(new UpdateCouponParams
            {
                CouponId = id,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinimumOrderAmount = request.MinimumOrderAmount,
                MaximumDiscountAmount = request.MaximumDiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UsageLimit = request.UsageLimit,
                IsActive = request.IsActive,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(coupon));
        }

        /// <summary>
        /// What a code would take off the caller's cart as it stands. Customer-facing: this
        /// is the "apply coupon" box on the basket page.
        ///
        /// A quotation and nothing more. No redemption is spent, so a client may call this as
        /// often as the customer retypes the box, and a code quoted successfully here can
        /// still be refused at checkout if a limited campaign runs out in between.
        ///
        /// The cart is the caller's own and the subtotal is read from it, so the discount
        /// quoted is the one checkout will work out from the same lines.
        /// </summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPost("preview")]
        [ProducesResponseType(typeof(CouponPreviewResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(PreviewCouponRequestValidator))]
        public async Task<IActionResult> Preview([FromBody] PreviewCouponRequest request)
        {
            var couponApplication = await couponService.Preview(new PreviewCouponParams
            {
                UserId = this.GetUserGuid(),
                Code = request.Code
            });

            return Ok(responseFormatter.Preview(couponApplication));
        }
    }
}
