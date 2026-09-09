namespace Domain.Exeptions;

/// <summary>
/// Raised when a review is left against an order that does not back it up: one that never
/// held the product, or one that was called off before anything was bought.
/// </summary>
public class ProductNotPurchasedException(string message) : Exception(message);
