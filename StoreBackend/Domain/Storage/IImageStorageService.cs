namespace Domain.Storage;

/// <summary>
/// Abstraction over the image hosting provider so the Domain layer
/// does not depend on a specific storage SDK or the RestApi presentation layer.
/// </summary>
public interface IImageStorageService
{
    Task<ImageStorageResult> Upload(UploadImageParams uploadImageParams);
    Task Delete(string publicId);
}
