using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        return new AuthResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token), 
        };
    }
}
