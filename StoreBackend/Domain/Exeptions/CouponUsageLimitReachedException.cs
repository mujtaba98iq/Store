namespace Domain.Exeptions;

/// <summary>
/// Raised when a coupon has been redeemed as many times as it was allowed. Also what a
/// customer sees when they lose a race for the last redemption of a limited campaign.
/// </summary>
public class CouponUsageLimitReachedException(string message) : DomainException(message);
