namespace Domain.Exeptions;

/// <summary>
/// Raised when something is created under a name or key another record already holds.
/// </summary>
/// <param name="resource">
/// What already exists, named the way a reader would name it - "User", "Product variant".
/// </param>
/// <param name="identifier">
/// Which one, phrased to read on from <paramref name="resource"/> - "username
/// hussen.iq", "SKU ABC-1". The caller sent this value in, so echoing it back gives
/// away nothing they did not already have.
/// </param>
public class ResourceAlreadyExistsException(string resource, string identifier)
    : DomainException($"{resource} with {identifier} already exists");
