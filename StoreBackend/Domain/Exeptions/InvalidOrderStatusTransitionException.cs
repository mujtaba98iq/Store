namespace Domain.Exeptions;

/// <summary>
/// Raised when an order is pushed to a status it cannot reach from where it is, such as
/// cancelling something already shipped.
/// </summary>
public class InvalidOrderStatusTransitionException(string message) : Exception(message);
