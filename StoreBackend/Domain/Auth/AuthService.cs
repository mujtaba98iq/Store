using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Auth;

public class AuthService(IUsersRepository usersRepository, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResult> Login(LoginParams loginParams)
    {
        var user = await usersRepository.FindByUsername(loginParams.Username)
            ?? throw new ResourceNotFoundException("User", $"No user found with username '{loginParams.Username}'");

        bool passwordValid = BCrypt.Net.BCrypt.Verify(loginParams.Password, user.Password);

        if (!passwordValid)
            throw new UnauthorizedAccessException("Invalid username or password.");


        var claims = new[]
            {
                // Unique identifier for the user
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),


                // user username address
                new Claim(ClaimTypes.Email, user.Username),


                // Role (user or Admin) used later for authorization
                new Claim("role", user.Role)
            };

        var key = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
               issuer: configuration["Jwt:Issuer"],
               audience: configuration["Jwt:Audience"],
               claims: claims,
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: creds
           );

        var refreshToken = GenerateRefreshToken();

        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.Now.AddDays(7);
        user.RefreshTokenRevokeAt = null;

        await usersRepository.Update(user);

        return new AuthResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken 
        };
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rag = RandomNumberGenerator.Create();
        rag.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<AuthResult> GetRefreshTokken(RefreshTokkenParams refreshTokkenParams)
    {
        var user = await usersRepository.FindByUsername(refreshTokkenParams.Email)
            ?? throw new ResourceNotFoundException("User", $"No user found with username '{refreshTokkenParams.Email}'");


        if(user.RefreshTokenRevokeAt != null)
            throw new UnauthorizedAccessException("Refresh token has been revoked.");

        if(user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        bool refreshValid = BCrypt.Net.BCrypt.Verify(refreshTokkenParams.RefreshToken, user.RefreshTokenHash);

        if(!refreshValid)
           throw new UnauthorizedAccessException("Invalid refresh token.");

        var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Username),
                new Claim("role", user.Role)
            };

        var key = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));

        var cards = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
               issuer: configuration["Jwt:Issuer"],
               audience: configuration["Jwt:Audience"],
               claims: claims,
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: cards
           );

        var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        var newRefreshToken = GenerateRefreshToken();

        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
        user.RefreshTokenExpiresAt = DateTime.Now.AddDays(7);
        user.RefreshTokenRevokeAt = null;

        await usersRepository.Update(user);


        return new AuthResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task Logout(LogoutParams logoutParams)
    {
        var user = await usersRepository.FindByUsername(logoutParams.Email)
            ?? throw new ResourceNotFoundException("User", $"No user found with username '{logoutParams.Email}'");

        bool refreshValid = BCrypt.Net.BCrypt.Verify(logoutParams.RefreshToken, user.RefreshTokenHash);
        if (refreshValid)
            return;

        user.RefreshTokenRevokeAt = DateTime.UtcNow;

        await usersRepository.Update(user);
        return;
    }
}
