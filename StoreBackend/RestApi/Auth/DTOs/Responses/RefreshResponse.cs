namespace RestApi.Auth;

public class RefreshResponse
{
    public required string AccessToken { get; set; }
    public string RefreshToken { get; set; }

}
