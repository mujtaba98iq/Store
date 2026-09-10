using Domain.Coupons;

namespace RestApi.Coupons;

public class CouponResponseFormatter : ICouponResponseFormatter
{
    public CouponListResponse Many(IEnumerable<Coupon> coupons, int totalCount)
    {
        return new CouponListResponse
        {
            Data = coupons.Select(One).ToList(),
            TotalCount = totalCount
        };
    }

    public CouponResponse One(Coupon coupon)
    {
        // Worked out here rather than stored, and worked out by the entity rather than by a
        // rule written out a second time in this file: whether a coupon can be redeemed is
        // the same question the service asks before letting one through, and the two must
        // not be able to give different answers.
        var now = DateTime.UtcNow;

        return new CouponResponse
        {
            Id = coupon.Id.ToString(),
            Code = coupon.Code,
            DiscountType = coupon.DiscountType.ToString(),
            DiscountValue = coupon.DiscountValue,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaximumDiscountAmount = coupon.MaximumDiscountAmount,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            IsActive = coupon.IsActive,
            IsRedeemable = coupon.IsRedeemableAt(now) && coupon.HasRedemptionsLeft(),
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt,
            CreatedById = coupon.CreatedById,
            UpdatedById = coupon.UpdatedById
        };
    }

    public CouponPreviewResponse Preview(CouponApplication couponApplication)
    {
        return new CouponPreviewResponse
        {
            Code = couponApplication.Coupon.Code,
            DiscountType = couponApplication.Coupon.DiscountType.ToString(),
            DiscountValue = couponApplication.Coupon.DiscountValue,
            Subtotal = couponApplication.Subtotal,
            DiscountAmount = couponApplication.DiscountAmount,
            Total = couponApplication.Total
        };
    }
}
