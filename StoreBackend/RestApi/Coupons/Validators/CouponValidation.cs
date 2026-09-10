namespace RestApi.Coupons;

/// <summary>
/// Limits shared by the coupon validators, so create and update cannot drift apart. What is
/// not here is anything a coupon's terms have to satisfy to make sense — a percentage over a
/// hundred, or a window that ends before it starts. Those are domain rules and the service
/// enforces them, because an update sends only the fields that changed and a validator
/// looking at one of them cannot tell what the coupon will look like afterwards.
/// </summary>
public static class CouponValidation
{
    /// <summary>
    /// Long enough for a readable campaign code, short enough to stay quotable over the
    /// phone.
    /// </summary>
    public const int CodeMinLength = 3;
    public const int CodeMaxLength = 40;

    /// <summary>
    /// Letters, digits, dashes and underscores. Spaces are out because a code is trimmed but
    /// not stripped, and a code with a space in the middle is one a customer will get wrong.
    /// </summary>
    public const string CodePattern = "^[A-Za-z0-9_-]+$";

    public const string CodePatternMessage = "Code may contain only letters, digits, dashes and underscores.";
}
