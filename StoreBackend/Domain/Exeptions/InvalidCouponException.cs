namespace Domain.Exeptions;

/// <summary>
/// Raised when the terms of a coupon do not hold together, such as a percentage over a
/// hundred or a window that ends before it starts. Staff-facing: it is about a coupon being
/// written, not about one being used.
/// </summary>
public class InvalidCouponException(string message) : DomainException(message);
