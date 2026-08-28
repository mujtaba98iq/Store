namespace RestApi.Setup;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Root folder every uploaded asset is stored under.
    /// </summary>
    public string Folder { get; set; } = "store";
}
