using System.Net;
using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Exeptions;
using Domain.Storage;
using Microsoft.Extensions.Options;

namespace RestApi.Setup;

public partial class CloudinaryImageStorageService(
    IOptions<CloudinarySettings> options,
    ILogger<CloudinaryImageStorageService> logger) : IImageStorageService
{
    private const string DeletedResult = "ok";
    private const string NotFoundResult = "not found";

    private readonly CloudinarySettings settings = options.Value;

    // Built on first use so a missing Cloudinary configuration only fails the
    // requests that actually store images, not every request of the module.
    private readonly Lazy<Cloudinary> client = new(() => CreateClient(options.Value));

    public async Task<ImageStorageResult> Upload(UploadImageParams uploadImageParams)
    {
        // The public id is generated here: the uploaded file name and extension are never trusted.
        var publicId = BuildPublicId(uploadImageParams.Folder);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(publicId, uploadImageParams.Content),
            PublicId = publicId,
            UseFilename = false,
            UniqueFilename = false,
            Overwrite = false,
            Invalidate = true
        };

        ImageUploadResult uploadResult;

        try
        {
            uploadResult = await client.Value.UploadAsync(uploadParams);
        }
        catch (Exception exception) when (exception is not ImageStorageException)
        {
            logger.LogError(exception, "Cloudinary upload failed for public id {PublicId}", publicId);
            throw new ImageStorageException("Failed to upload the image to Cloudinary.");
        }

        if (uploadResult.Error is not null || uploadResult.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError(
                "Cloudinary upload failed for public id {PublicId} with status {StatusCode}: {Error}",
                publicId,
                uploadResult.StatusCode,
                uploadResult.Error?.Message);

            throw new ImageStorageException($"Failed to upload the image to Cloudinary: {uploadResult.Error?.Message ?? uploadResult.StatusCode.ToString()}");
        }

        var imageUrl = uploadResult.SecureUrl?.AbsoluteUri ?? uploadResult.Url?.AbsoluteUri
            ?? throw new ImageStorageException("Cloudinary did not return a URL for the uploaded image.");

        return new ImageStorageResult
        {
            ImageUrl = imageUrl,
            PublicId = uploadResult.PublicId
        };
    }

    public async Task Delete(string publicId)
    {
        var deletionParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image,
            Invalidate = true
        };

        DeletionResult deletionResult;

        try
        {
            deletionResult = await client.Value.DestroyAsync(deletionParams);
        }
        catch (Exception exception) when (exception is not ImageStorageException)
        {
            logger.LogError(exception, "Cloudinary delete failed for public id {PublicId}", publicId);
            throw new ImageStorageException("Failed to delete the image from Cloudinary.");
        }

        // An asset that is already gone is not an error, the desired state is reached either way.
        if (deletionResult.Result is DeletedResult or NotFoundResult)
        {
            return;
        }

        logger.LogError(
            "Cloudinary delete failed for public id {PublicId} with status {StatusCode}: {Result}",
            publicId,
            deletionResult.StatusCode,
            deletionResult.Error?.Message ?? deletionResult.Result);

        throw new ImageStorageException($"Failed to delete the image from Cloudinary: {deletionResult.Error?.Message ?? deletionResult.Result}");
    }

    private string BuildPublicId(string? folder)
    {
        var segments = new[] { settings.Folder, folder }
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .Select(segment => SanitizeFolder(segment!))
            .Where(segment => segment.Length > 0)
            .Append(Guid.NewGuid().ToString("N"));

        return string.Join('/', segments);
    }

    private static string SanitizeFolder(string folder)
    {
        return UnsafeFolderCharacters().Replace(folder, string.Empty).Trim('/');
    }

    private static Cloudinary CreateClient(CloudinarySettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.CloudName)
            || string.IsNullOrWhiteSpace(settings.ApiKey)
            || string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            throw new ImageStorageException(
                "Cloudinary is not configured. Set Cloudinary:CloudName, Cloudinary:ApiKey and Cloudinary:ApiSecret.");
        }

        return new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
    }

    [GeneratedRegex(@"[^A-Za-z0-9_\-/]")]
    private static partial Regex UnsafeFolderCharacters();
}
