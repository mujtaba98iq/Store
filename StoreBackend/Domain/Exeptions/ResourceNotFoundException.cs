namespace Domain;

public class ResourceNotFoundException(string resource, string identifier) : Exception($"Could not find {resource} with identifier: ${identifier}");
