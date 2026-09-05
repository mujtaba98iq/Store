namespace Domain.Exeptions;

/// <summary>
/// Raised when a checkout is attempted against a cart with nothing in it.
/// </summary>
public class EmptyCartCheckoutException(string message) : Exception(message);
