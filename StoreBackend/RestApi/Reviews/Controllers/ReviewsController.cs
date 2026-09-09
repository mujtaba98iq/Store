using Domain.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Reviews.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(
        IReviewService reviewService,
        IReviewResponseFormatter responseFormatter,
        IAuthorizationService authorizationService) : ControllerBase
    {
        /// <summary>
        /// Every review, published or not. Staff only, and the moderation queue: what is still
        /// waiting on somebody is everything with isApproved=false.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ReviewListResponse), 200)]
        public async Task<IActionResult> GetAll([FromQuery] ReviewFilters reviewFilters)
        {
            var reviews = await reviewService.Search(reviewFilters);
            return Ok(responseFormatter.Many(reviews.Data, reviews.TotalCount));
        }

        /// <summary>
        /// The reviews on one product's page. Only what staff have cleared: this is the read
        /// every customer makes, and an unmoderated review has no business appearing in it.
        /// </summary>
        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(typeof(ReviewListResponse), 200)]
        public async Task<IActionResult> GetByProductId(Guid productId, [FromQuery] ReviewFilters reviewFilters)
        {
            // Both overwritten rather than read from the query, so this route can only ever
            // return published reviews of the product named in the path, however the filter
            // arrived.
            reviewFilters.ProductId = productId;
            reviewFilters.IsApproved = true;

            var reviews = await reviewService.Search(reviewFilters);
            return Ok(responseFormatter.Many(reviews.Data, reviews.TotalCount));
        }

        /// <summary>
        /// Everything the caller has ever written, whether or not it has been published. It
        /// is the only place an author can see a review of theirs that is still held back.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ReviewListResponse), 200)]
        public async Task<IActionResult> GetMine([FromQuery] ReviewFilters reviewFilters)
        {
            reviewFilters.UserId = this.GetUserGuid();

            var reviews = await reviewService.Search(reviewFilters);
            return Ok(responseFormatter.Many(reviews.Data, reviews.TotalCount));
        }

        /// <summary>
        /// Records what the caller thought of something they bought. The order has to be
        /// theirs and has to have included the product, which is what keeps this from being
        /// a rating anybody can leave on anything.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReviewResponse), 201)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(CreateReviewRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
        {
            var review = await reviewService.Create(new CreateReviewParams
            {
                UserId = this.GetUserGuid(),
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedById = this.GetUserId()
            });

            return CreatedAtAction(nameof(GetById), new { id = review.Id }, responseFormatter.One(review));
        }

        /// <summary>
        /// Lets the author change their mind. Author only, admins included: moderation is
        /// approving or holding a review, never rewriting somebody's words. A review that
        /// belongs to anybody else reads as missing rather than refused.
        ///
        /// The edit sends the review back to the queue, so what readers see is always the
        /// version staff cleared.
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(ReviewResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateReviewRequestValidator))]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequest request)
        {
            var review = await reviewService.Update(new UpdateReviewParams
            {
                ReviewId = id,
                UserId = this.GetUserGuid(),
                Rating = request.Rating,
                Comment = request.Comment,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(review));
        }

        /// <summary>
        /// Publishes a review or takes it back down. Staff only: what goes on a product page
        /// is not the author's decision.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/approval")]
        [ProducesResponseType(typeof(ReviewResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> SetApproval(Guid id, [FromBody] SetReviewApprovalRequest request)
        {
            var review = await reviewService.SetApproval(new SetReviewApprovalParams
            {
                ReviewId = id,
                IsApproved = request.IsApproved,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(review));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ReviewResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var review = await reviewService.FindById(id);

            if (review is null)
            {
                return NotFound();
            }

            // A published review is public: it is already on the product page, so there is
            // nothing to guard.
            if (review.IsApproved)
            {
                return Ok(responseFormatter.One(review));
            }

            // One that is still held back belongs to its author and to staff. Reported as
            // missing rather than refused, because whether somebody wrote a review that is
            // being withheld is not something a stranger should be able to learn.
            var authResult = await authorizationService.AuthorizeAsync(User, review.UserId, "UserOwnerOrAdminPolicy");

            return authResult.Succeeded
                ? Ok(responseFormatter.One(review))
                : NotFound();
        }
    }
}
