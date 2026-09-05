namespace Domain.Exeptions;

/// <summary>
/// Raised when a parcel is pushed to a status it cannot reach from where it stands, such as
/// delivering something that never left.
/// </summary>
public class InvalidShipmentStatusTransitionException(string message) : Exception(message);
