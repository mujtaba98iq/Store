namespace Domain.Auth;

/// <summary>
/// Abstraction over JWT generation so the Domain layer
/// does not depend on the RestApi presentation layer.
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string username, string role);
}
