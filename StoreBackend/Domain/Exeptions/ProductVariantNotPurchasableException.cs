namespace Domain.Exeptions;

/// <summary>
/// Raised when a variant exists but cannot be put in a cart, because it was deactivated
/// or because nobody has given it a price yet.
/// </summary>
public class ProductVariantNotPurchasableException(string message) : Exception(message);
