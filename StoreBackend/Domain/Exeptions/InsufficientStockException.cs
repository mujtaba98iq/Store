namespace Domain.Exeptions;

/// <summary>
/// Raised when a checkout asks for more units than are free to reserve, either because
/// stock is short or because the variant has never been stocked.
/// </summary>
public class InsufficientStockException(string message) : Exception(message);
