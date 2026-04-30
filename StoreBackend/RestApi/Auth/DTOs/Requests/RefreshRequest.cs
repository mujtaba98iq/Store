namespace RestApi.Auth;

public class RefreshRequest
{
    public required string RefreshToken { get; set; }
    public string Email { get; set; }
}
