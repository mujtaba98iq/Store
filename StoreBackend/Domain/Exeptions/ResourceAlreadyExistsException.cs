namespace Domain.Exeptions;

public class ResourceAlreadyExistsException(string identifier) : Exception($"User with username ${identifier} already exists ");