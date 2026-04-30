namespace Domain.Auth;

public class AuthResult
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
