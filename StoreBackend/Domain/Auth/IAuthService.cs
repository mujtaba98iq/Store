
namespace Domain.Auth;

public interface IAuthService
{
    Task<AuthResult> Login(LoginParams loginParams);
    Task<AuthResult> GetRefreshTokken(RefreshTokkenParams refreshTokkenParams);
    Task Logout(LogoutParams logoutParams);
}
