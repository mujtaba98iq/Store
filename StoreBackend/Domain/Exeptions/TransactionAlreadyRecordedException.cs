namespace Domain.Exeptions;

/// <summary>
/// Raised when a provider reference is banked against a second payment. The database refuses
/// it too; this catches it first, with a message that says what happened.
/// </summary>
public class TransactionAlreadyRecordedException(string message) : Exception(message);
