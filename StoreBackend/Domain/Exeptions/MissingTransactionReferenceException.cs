namespace Domain.Exeptions;

/// <summary>
/// Raised when a payment is settled without the provider reference that proves it. Cash on
/// delivery is the exception: the courier is the receipt.
/// </summary>
public class MissingTransactionReferenceException(string message) : Exception(message);
