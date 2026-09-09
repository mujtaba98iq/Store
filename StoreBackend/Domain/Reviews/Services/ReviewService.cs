using Domain.Exeptions;
using Domain.Orders;
using Domain.Products;
using Sheard.Type;

namespace Domain.Reviews
{
    public class ReviewService(
        IReviewsRepository reviewsRepository,
        IProductsRepository productsRepository,
        IOrdersRepository ordersRepository,
        IOrderItemsRepository orderItemsRepository) : IReviewService
    {
        public async Task<Review> Create(CreateReviewParams createReviewParams)
        {
            _ = await productsRepository.FindById(createReviewParams.ProductId)
                ?? throw new ResourceNotFoundException("Product", $"Product with ID {createReviewParams.ProductId} not found");

            EnsureRatingIsInRange(createReviewParams.Rating);

            await EnsureProductWasPurchased(createReviewParams.UserId, createReviewParams.ProductId, createReviewParams.OrderId);
            await EnsureProductIsNotReviewedYet(createReviewParams.UserId, createReviewParams.ProductId);

            var review = await reviewsRepository.Create(new Review
            {
                Id = Guid.NewGuid(),
                UserId = createReviewParams.UserId,
                ProductId = createReviewParams.ProductId,
                OrderId = createReviewParams.OrderId,
                Rating = createReviewParams.Rating,
                Comment = NormaliseComment(createReviewParams.Comment),

                // Nothing a customer types reaches the product page on their own say-so.
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createReviewParams.CreatedById
            });

            return await reviewsRepository.FindById(review.Id) ?? review;
        }

        public async Task<Review?> FindById(Guid id)
        {
            return await reviewsRepository.FindById(id);
        }

        public async Task<PaginationResult<Review>> Search(ReviewFilters reviewFilters)
        {
            var reviews = await reviewsRepository.FindByFilters(reviewFilters);
            var totalCount = await reviewsRepository.GetTotalCountByFilters(reviewFilters);

            return new PaginationResult<Review>
            {
                TotalCount = totalCount,
                Data = reviews
            };
        }

        public async Task<Review> Update(UpdateReviewParams updateReviewParams)
        {
            var review = await reviewsRepository.FindById(updateReviewParams.ReviewId)
                         ?? throw new ResourceNotFoundException("Review", $"Review with ID {updateReviewParams.ReviewId} not found");

            // Somebody else's review is reported as missing rather than refused, as an order
            // is: whether it exists is not something a stranger should be able to learn.
            if (review.UserId != updateReviewParams.UserId)
            {
                throw new ResourceNotFoundException("Review", $"Review with ID {updateReviewParams.ReviewId} not found");
            }

            var rating = updateReviewParams.Rating ?? review.Rating;
            EnsureRatingIsInRange(rating);

            review.Rating = rating;
            review.Comment = NormaliseComment(updateReviewParams.Comment) ?? review.Comment;

            // Straight back into the queue. An approved review that could be rewritten
            // afterwards would be a hole in the moderation: what staff cleared and what
            // readers see would no longer be the same words.
            review.IsApproved = false;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedById = updateReviewParams.UpdatedById;

            await reviewsRepository.Update(review);

            return await reviewsRepository.FindById(review.Id) ?? review;
        }

        public async Task<Review> SetApproval(SetReviewApprovalParams setReviewApprovalParams)
        {
            var review = await reviewsRepository.FindById(setReviewApprovalParams.ReviewId)
                         ?? throw new ResourceNotFoundException("Review", $"Review with ID {setReviewApprovalParams.ReviewId} not found");

            // Written even when it already stands that way, rather than refused as a payment
            // would be. Clearing something twice is harmless, and the audit columns are then
            // a record of who last looked at it.
            review.IsApproved = setReviewApprovalParams.IsApproved;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedById = setReviewApprovalParams.UpdatedById;

            await reviewsRepository.Update(review);

            return await reviewsRepository.FindById(review.Id) ?? review;
        }

        /// <summary>
        /// The check the OrderId on a review exists for: the customer has to have bought the
        /// thing they are rating, on the order they are pointing at.
        ///
        /// A cancelled order is refused because nothing on it was ever delivered. Anything
        /// short of that is allowed, so a customer is not kept waiting on staff marking the
        /// parcel delivered before they can say what they think.
        /// </summary>
        private async Task EnsureProductWasPurchased(Guid userId, Guid productId, Guid orderId)
        {
            var order = await ordersRepository.FindById(orderId)
                        ?? throw new ResourceNotFoundException("Order", $"Order with ID {orderId} not found");

            if (order.UserId != userId)
            {
                throw new ResourceNotFoundException("Order", $"Order with ID {orderId} not found");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new ProductNotPurchasedException($"Order {order.OrderNumber} was cancelled, so nothing on it can be reviewed.");
            }

            if (!await orderItemsRepository.ContainsProduct(orderId, productId))
            {
                throw new ProductNotPurchasedException($"Order {order.OrderNumber} does not include the product being reviewed.");
            }
        }

        /// <summary>
        /// Caught here rather than left to the unique index, so a second attempt reads as the
        /// duplicate it is. Buying the product again does not earn another review: a customer
        /// who has changed their mind revises the one they already hold.
        /// </summary>
        private async Task EnsureProductIsNotReviewedYet(Guid userId, Guid productId)
        {
            var existing = await reviewsRepository.FindByUserAndProduct(userId, productId);

            if (existing != null)
            {
                throw new ProductAlreadyReviewedException(
                    $"Product {productId} has already been reviewed by this customer. Edit review {existing.Id} instead.");
            }
        }

        /// <summary>
        /// A comment of nothing but spaces is no comment at all, and is stored as none rather
        /// than as blank text a product page would have to render.
        /// </summary>
        private static string? NormaliseComment(string? comment)
        {
            return string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        }

        private static void EnsureRatingIsInRange(int rating)
        {
            if (rating is < Review.MinRating or > Review.MaxRating)
            {
                throw new InvalidReviewRatingException(
                    $"Rating must be between {Review.MinRating} and {Review.MaxRating}, but was {rating}.");
            }
        }
    }
}
