namespace Domain.Exeptions;

/// <summary>
/// Raised when a fresh payment is recorded against an order that has already been settled.
/// Taking the money twice is worse than refusing the second attempt.
/// </summary>
public class OrderAlreadyPaidException(string message) : Exception(message);
