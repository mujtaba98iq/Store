using Sheard.Type;

namespace Domain.Coupons;

public class CouponFilters : ListingOptions
{
    public Guid? CouponId { get; set; }

    /// <summary>
    /// Matched whole rather than as a fragment, and normalised before the comparison: a code
    /// is an exact thing a customer quotes, not a term to search on.
    /// </summary>
    public string? Code { get; set; }

    public DiscountType? DiscountType { get; set; }

    /// <summary>
    /// The switch on its own, ignoring the dates. False is the list of campaigns staff have
    /// withdrawn, which is not the same as the list of ones that have expired.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// True keeps only what a customer could redeem this moment: switched on, inside its
    /// window, and with redemptions left. False keeps everything that fails any of those.
    /// It is the filter a staff dashboard's "live campaigns" tab wants, and the one thing
    /// here that <see cref="IsActive"/> cannot answer on its own.
    /// </summary>
    public bool? IsRedeemable { get; set; }

    /// <summary>
    /// Bounds on the coupon's own window rather than on when the row was written: coupons
    /// whose window has any overlap with the range given. Either end may be left out.
    /// </summary>
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }

    /// <summary>
    /// Bounds on when the coupon was set up. Inclusive at both ends.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public CouponOrderBy? OrderBy { get; set; } = CouponOrderBy.CreatedAt;
}
