namespace Domain.Exeptions;

/// <summary>
/// Raised when a second shipment is opened for an order that already has one. Two of them
/// could not agree on where the parcel is.
/// </summary>
public class ShipmentAlreadyExistsException(string message) : Exception(message);
