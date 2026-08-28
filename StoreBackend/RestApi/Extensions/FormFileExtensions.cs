namespace RestApi.Extensions;

/// <summary>
/// Upload rules for image files. The original file name and extension are never
/// trusted: the declared content type and the actual file signature must both
/// match a supported image format.
/// </summary>
public static class FormFileExtensions
{
    public const int MaxImageSizeInMegabytes = 5;
    public const long MaxImageSizeInBytes = MaxImageSizeInMegabytes * 1024L * 1024L;

    /// <summary>
    /// Allows for the multipart envelope around the file itself.
    /// </summary>
    public const long MaxImageRequestSizeInBytes = MaxImageSizeInBytes + 512L * 1024L;

    private const int SignatureLength = 12;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    ];

    public static string AllowedImageContentTypesDescription => string.Join(", ", AllowedContentTypes);

    public static bool HasAllowedImageContentType(this IFormFile file)
    {
        var contentType = file.ContentType?.Split(';')[0].Trim();
        return contentType is not null && AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    public static bool HasAllowedImageContent(this IFormFile file)
    {
        Span<byte> buffer = stackalloc byte[SignatureLength];

        using var stream = file.OpenReadStream();
        var readCount = stream.ReadAtLeast(buffer, SignatureLength, throwOnEndOfStream: false);

        ReadOnlySpan<byte> signature = buffer[..readCount];

        return IsJpeg(signature) || IsPng(signature) || IsWebp(signature);
    }

    private static bool IsJpeg(ReadOnlySpan<byte> signature)
    {
        ReadOnlySpan<byte> jpeg = [0xFF, 0xD8, 0xFF];
        return signature.StartsWith(jpeg);
    }

    private static bool IsPng(ReadOnlySpan<byte> signature)
    {
        ReadOnlySpan<byte> png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        return signature.StartsWith(png);
    }

    private static bool IsWebp(ReadOnlySpan<byte> signature)
    {
        return signature.Length >= SignatureLength
               && signature.StartsWith("RIFF"u8)
               && signature[8..SignatureLength].SequenceEqual("WEBP"u8);
    }
}
