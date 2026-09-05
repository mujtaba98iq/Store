namespace Domain.Exeptions;

/// <summary>
/// Raised when a payment is pushed to a status it cannot reach from where it stands, such as
/// refunding money that never arrived.
/// </summary>
public class InvalidPaymentStatusTransitionException(string message) : Exception(message);
