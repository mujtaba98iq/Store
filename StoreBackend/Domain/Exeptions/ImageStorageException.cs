namespace Domain.Exeptions;

/// <summary>
/// Raised when the image store will not take a file, or will not give one up.
/// </summary>
/// <remarks>
/// Deliberately not a <see cref="DomainException"/>. These messages name the storage
/// provider and quote its own error text back, which is about the inside of the
/// server rather than about anything the caller did - so the API answers it with a
/// sentence of its own and keeps this one for the log.
/// </remarks>
public class ImageStorageException(string message) : Exception(message);
