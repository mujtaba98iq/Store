namespace Domain.Exeptions;

/// <summary>
/// Raised when money is offered for an order that is in no position to take it, such as one
/// that has already been called off.
/// </summary>
public class OrderNotPayableException(string message) : Exception(message);
