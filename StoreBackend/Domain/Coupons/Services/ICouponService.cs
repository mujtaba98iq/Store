using Sheard.Type;

namespace Domain.Coupons;

public interface ICouponService
{
    /// <summary>
    /// Sets up a campaign. Refused if the terms do not hold together, or if the code is one
    /// another coupon already answers to.
    /// </summary>
    Task<Coupon> Create(CreateCouponParams createCouponParams);

    Task<Coupon?> FindById(Guid id);

    /// <summary>
    /// Looks a coupon up the way a customer quotes it, capitalisation and stray spaces and
    /// all. Says nothing about whether it may be used.
    /// </summary>
    Task<Coupon?> FindByCode(string code);

    Task<PaginationResult<Coupon>> Search(CouponFilters couponFilters);

    /// <summary>
    /// Revises a campaign's terms. The code is not among them: orders keep their own copy of
    /// what was quoted, and a renamed coupon would leave the two disagreeing.
    /// </summary>
    Task<Coupon> Update(UpdateCouponParams updateCouponParams);

    /// <summary>
    /// Works out what a coupon would take off an order of the given size, and throws if it
    /// would take off nothing because the coupon cannot be used. Changes nothing: the
    /// redemption is not booked, so this may be called as often as a client likes.
    /// </summary>
    Task<CouponApplication> Apply(ApplyCouponParams applyCouponParams);

    /// <summary>
    /// The same question asked against the caller's own cart, which is where the customer
    /// asks it from. The subtotal comes off the cart rather than off the request.
    /// </summary>
    Task<CouponApplication> Preview(PreviewCouponParams previewCouponParams);

    /// <summary>
    /// Spends a redemption and returns what came off. This is the call checkout makes, and
    /// the only one that moves <see cref="Coupon.UsedCount"/>.
    ///
    /// The limit is enforced by the write itself, so a customer who loses a race for the
    /// last redemption of a campaign is refused here rather than quietly allowed through.
    /// </summary>
    Task<CouponApplication> Redeem(RedeemCouponParams redeemCouponParams);
}
