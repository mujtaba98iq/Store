namespace Domain.Exeptions;

public class InvalidCartQuantityException(string message) : DomainException(message);
