using Domain.Auth;
using Microsoft.AspNetCore.Mvc;
using UseValidator;

namespace RestApi.Auth.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    [UseBodyValidator(Validator = typeof(LoginRequestValidator))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await authService.Login(new LoginParams
            {
                Username = request.Username,
                Password = request.Password
            });

            return Ok(new LoginResponse
            {
                AccessToken = result.AccessToken
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }
    }
}
