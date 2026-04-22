namespace Domain.Auth;

public interface IAuthService
{
    Task<AuthResult> Login(LoginParams loginParams);
}
