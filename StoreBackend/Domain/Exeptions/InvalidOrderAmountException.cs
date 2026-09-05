namespace Domain.Exeptions;

/// <summary>
/// Raised when the money on an order does not add up, such as a discount larger than the
/// goods it is taken off.
/// </summary>
public class InvalidOrderAmountException(string message) : Exception(message);
