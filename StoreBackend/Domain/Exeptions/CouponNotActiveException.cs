namespace Domain.Exeptions;

/// <summary>
/// Raised when a coupon exists but is in no position to be used: withdrawn by staff, or
/// quoted before it opens or after it has closed.
/// </summary>
public class CouponNotActiveException(string message) : Exception(message);
