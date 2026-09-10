using Domain.Coupons;

namespace RestApi.Coupons;

public interface ICouponResponseFormatter
{
    CouponResponse One(Coupon coupon);
    CouponListResponse Many(IEnumerable<Coupon> coupons, int totalCount);
    CouponPreviewResponse Preview(CouponApplication couponApplication);
}
