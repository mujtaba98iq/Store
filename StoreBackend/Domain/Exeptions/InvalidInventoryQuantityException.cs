namespace Domain.Exeptions;

public class InvalidInventoryQuantityException(string message) : Exception(message);
