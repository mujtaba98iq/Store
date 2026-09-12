namespace Domain.Exeptions;

public class InvalidInventoryQuantityException(string message) : DomainException(message);
