using Domain.Carts;
using Domain.Exeptions;
using Sheard.Type;

namespace Domain.Coupons
{
    public class CouponService(
        ICouponsRepository couponsRepository,
        ICartService cartService) : ICouponService
    {
        public async Task<Coupon> Create(CreateCouponParams createCouponParams)
        {
            var code = Coupon.NormaliseCode(createCouponParams.Code);

            EnsureTermsHoldTogether(
                createCouponParams.DiscountType,
                createCouponParams.DiscountValue,
                createCouponParams.MinimumOrderAmount,
                createCouponParams.MaximumDiscountAmount,
                createCouponParams.StartDate,
                createCouponParams.EndDate,
                createCouponParams.UsageLimit);

            await EnsureCodeIsFree(code);

            var coupon = await couponsRepository.Create(new Coupon
            {
                Id = Guid.NewGuid(),
                Code = code,
                DiscountType = createCouponParams.DiscountType,
                DiscountValue = createCouponParams.DiscountValue,
                MinimumOrderAmount = createCouponParams.MinimumOrderAmount,
                MaximumDiscountAmount = createCouponParams.MaximumDiscountAmount,
                StartDate = createCouponParams.StartDate,
                EndDate = createCouponParams.EndDate,
                UsageLimit = createCouponParams.UsageLimit,

                // A fresh campaign has been redeemed by nobody. Never taken from the caller:
                // the count is a record of what happened, not a term staff get to set.
                UsedCount = 0,
                IsActive = createCouponParams.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createCouponParams.CreatedById
            });

            return await couponsRepository.FindById(coupon.Id) ?? coupon;
        }

        public async Task<Coupon?> FindById(Guid id)
        {
            return await couponsRepository.FindById(id);
        }

        public async Task<Coupon?> FindByCode(string code)
        {
            return await couponsRepository.FindByCode(Coupon.NormaliseCode(code));
        }

        public async Task<PaginationResult<Coupon>> Search(CouponFilters couponFilters)
        {
            var coupons = await couponsRepository.FindByFilters(couponFilters);
            var totalCount = await couponsRepository.GetTotalCountByFilters(couponFilters);

            return new PaginationResult<Coupon>
            {
                TotalCount = totalCount,
                Data = coupons
            };
        }

        public async Task<Coupon> Update(UpdateCouponParams updateCouponParams)
        {
            var coupon = await couponsRepository.FindById(updateCouponParams.CouponId)
                         ?? throw new ResourceNotFoundException("Coupon", $"Coupon with ID {updateCouponParams.CouponId} not found");

            var discountType = updateCouponParams.DiscountType ?? coupon.DiscountType;
            var discountValue = updateCouponParams.DiscountValue ?? coupon.DiscountValue;
            var minimumOrderAmount = updateCouponParams.MinimumOrderAmount ?? coupon.MinimumOrderAmount;
            var maximumDiscountAmount = updateCouponParams.MaximumDiscountAmount ?? coupon.MaximumDiscountAmount;
            var startDate = updateCouponParams.StartDate ?? coupon.StartDate;
            var endDate = updateCouponParams.EndDate ?? coupon.EndDate;
            var usageLimit = updateCouponParams.UsageLimit ?? coupon.UsageLimit;

            // Checked on the coupon as it will stand, not on the fields that arrived. An edit
            // that only moves the end date can still produce a window that ends before it
            // starts, and only the merged version can show that.
            EnsureTermsHoldTogether(discountType, discountValue, minimumOrderAmount, maximumDiscountAmount, startDate, endDate, usageLimit);

            // A limit cut below what has already gone out would put the coupon permanently
            // over its own ceiling. Refused rather than clamped: staff meant something by the
            // number they typed, and silently raising it back up would hide that it was
            // impossible.
            if (usageLimit.HasValue && usageLimit.Value < coupon.UsedCount)
            {
                throw new InvalidCouponException(
                    $"UsageLimit ({usageLimit.Value}) cannot be lower than the {coupon.UsedCount} redemption(s) already made. Deactivate the coupon instead.");
            }

            coupon.DiscountType = discountType;
            coupon.DiscountValue = discountValue;
            coupon.MinimumOrderAmount = minimumOrderAmount;
            coupon.MaximumDiscountAmount = maximumDiscountAmount;
            coupon.StartDate = startDate;
            coupon.EndDate = endDate;
            coupon.UsageLimit = usageLimit;
            coupon.IsActive = updateCouponParams.IsActive ?? coupon.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedById = updateCouponParams.UpdatedById;

            await couponsRepository.Update(coupon);

            return await couponsRepository.FindById(coupon.Id) ?? coupon;
        }

        public async Task<CouponApplication> Apply(ApplyCouponParams applyCouponParams)
        {
            var coupon = await FindRedeemableOrThrow(applyCouponParams.Code, applyCouponParams.Subtotal);

            return BuildApplication(coupon, applyCouponParams.Subtotal);
        }

        public async Task<CouponApplication> Preview(PreviewCouponParams previewCouponParams)
        {
            var cart = await cartService.FindByUserId(previewCouponParams.UserId)
                       ?? throw new ResourceNotFoundException("Cart", $"Cart for user with ID {previewCouponParams.UserId} not found");

            // The cart's own total, which is the sum of its live lines. Checkout re-reads the
            // same lines a moment later, so a preview and the order that follows it agree
            // unless the customer changes the cart in between.
            return await Apply(new ApplyCouponParams
            {
                Code = previewCouponParams.Code,
                Subtotal = cart.TotalAmount
            });
        }

        public async Task<CouponApplication> Redeem(RedeemCouponParams redeemCouponParams)
        {
            var coupon = await FindRedeemableOrThrow(redeemCouponParams.Code, redeemCouponParams.Subtotal);

            // The count read a moment ago is already stale, so the limit is enforced again by
            // the write itself. Losing this is not a fault in the caller: it is two customers
            // reaching for the last redemption at once, and one of them has to be told the
            // campaign is finished.
            if (!await couponsRepository.TryConsume(coupon.Id))
            {
                throw new CouponUsageLimitReachedException($"Coupon {coupon.Code} has been fully redeemed.");
            }

            // Re-read so the caller sees the count it just moved rather than the one loaded
            // before the write.
            var redeemed = await couponsRepository.FindById(coupon.Id) ?? coupon;

            return BuildApplication(redeemed, redeemCouponParams.Subtotal);
        }

        /// <summary>
        /// Every reason a coupon may not be used, in the order a customer can do something
        /// about them: one that does not exist or has been withdrawn is a dead end, while one
        /// refused on the minimum can be met by adding to the basket.
        /// </summary>
        private async Task<Coupon> FindRedeemableOrThrow(string code, decimal subtotal)
        {
            var normalisedCode = Coupon.NormaliseCode(code);

            var coupon = await couponsRepository.FindByCode(normalisedCode)
                         ?? throw new ResourceNotFoundException("Coupon", $"Coupon with code {normalisedCode} not found");

            var now = DateTime.UtcNow;

            if (!coupon.IsActive)
            {
                throw new CouponNotActiveException($"Coupon {coupon.Code} is not active.");
            }

            // The two ends of the window are reported apart. "Not yet" is worth waiting for
            // and "no longer" is not, and a customer holding a code for a campaign that opens
            // tomorrow should be told which of the two they are looking at.
            if (now < coupon.StartDate)
            {
                throw new CouponNotActiveException($"Coupon {coupon.Code} cannot be used before {coupon.StartDate:u}.");
            }

            if (now > coupon.EndDate)
            {
                throw new CouponNotActiveException($"Coupon {coupon.Code} expired on {coupon.EndDate:u}.");
            }

            if (!coupon.HasRedemptionsLeft())
            {
                throw new CouponUsageLimitReachedException(
                    $"Coupon {coupon.Code} has been redeemed {coupon.UsedCount} time(s), which is its limit.");
            }

            // Judged on the goods, so whether an order qualifies does not change with how far
            // it is being posted.
            if (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount.Value)
            {
                throw new CouponMinimumOrderAmountException(
                    $"Coupon {coupon.Code} requires an order of at least {coupon.MinimumOrderAmount.Value}, but the order comes to {subtotal}.");
            }

            return coupon;
        }

        private static CouponApplication BuildApplication(Coupon coupon, decimal subtotal)
        {
            return new CouponApplication
            {
                Coupon = coupon,
                Subtotal = subtotal,
                DiscountAmount = coupon.CalculateDiscountFor(subtotal)
            };
        }

        private async Task EnsureCodeIsFree(string code)
        {
            var existing = await couponsRepository.FindByCode(code);

            if (existing != null)
            {
                throw new CouponCodeAlreadyExistsException(
                    $"Coupon code {code} is already in use by coupon {existing.Id}.");
            }
        }

        /// <summary>
        /// The terms a coupon has to satisfy to be worth writing down. Each of these would
        /// otherwise leave a row in the table doing something nobody intended: a discount of
        /// nothing, a percentage over a hundred that hands the customer money back, or a
        /// window no moment falls inside.
        /// </summary>
        private static void EnsureTermsHoldTogether(
            DiscountType discountType,
            decimal discountValue,
            decimal? minimumOrderAmount,
            decimal? maximumDiscountAmount,
            DateTime startDate,
            DateTime endDate,
            int? usageLimit)
        {
            if (discountValue <= 0)
            {
                throw new InvalidCouponException("DiscountValue must be greater than zero.");
            }

            if (discountType == DiscountType.Percentage && discountValue > Coupon.MaxPercentageValue)
            {
                throw new InvalidCouponException(
                    $"A percentage coupon cannot exceed {Coupon.MaxPercentageValue}, but DiscountValue was {discountValue}.");
            }

            if (minimumOrderAmount < 0)
            {
                throw new InvalidCouponException("MinimumOrderAmount cannot be negative.");
            }

            if (maximumDiscountAmount <= 0)
            {
                throw new InvalidCouponException("MaximumDiscountAmount must be greater than zero when set.");
            }

            // A cap belongs on a percentage. It is honoured on a fixed amount all the same,
            // but a cap below the amount itself means the amount is a fiction, and that is
            // worth refusing rather than quietly applying.
            if (discountType == DiscountType.FixedAmount && maximumDiscountAmount < discountValue)
            {
                throw new InvalidCouponException(
                    $"MaximumDiscountAmount ({maximumDiscountAmount}) is below the fixed DiscountValue ({discountValue}), so the coupon could never pay out what it says.");
            }

            if (endDate <= startDate)
            {
                throw new InvalidCouponException($"EndDate ({endDate:u}) must be after StartDate ({startDate:u}).");
            }

            if (usageLimit <= 0)
            {
                throw new InvalidCouponException("UsageLimit must be greater than zero when set. Leave it empty for an unlimited coupon.");
            }
        }
    }
}
