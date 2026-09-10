namespace Domain.Exeptions;

/// <summary>
/// Raised when a coupon is given a code another one already holds. Caught before the write
/// so the clash reads as the duplicate it is rather than as a database error.
/// </summary>
public class CouponCodeAlreadyExistsException(string message) : Exception(message);
