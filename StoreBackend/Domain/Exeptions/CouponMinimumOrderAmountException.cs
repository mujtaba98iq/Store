namespace Domain.Exeptions;

/// <summary>
/// Raised when a coupon is quoted on an order too small to qualify for it. The shortfall is
/// in the message, because the customer can act on it by adding to the basket.
/// </summary>
public class CouponMinimumOrderAmountException(string message) : Exception(message);
