namespace Domain.Storage;

public class UploadImageParams
{
    public required Stream Content { get; set; }

    /// <summary>
    /// Optional folder the image is stored under. The storage service always
    /// generates the file identifier itself, the caller never supplies one.
    /// </summary>
    public string? Folder { get; set; }
}
