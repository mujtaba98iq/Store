using Domain.Auth;
using Microsoft.AspNetCore.Mvc;
using UseValidator;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Bad credentials leave here as UnauthorizedAccessException and are answered as
        // a 401 by GlobalExceptionHandler, which gives every way of failing to sign in
        // the same sentence - so whether a username exists cannot be read off the reply.
        var result = await authService.Login(new LoginParams
        {
            Username = request.Username,
            Password = request.Password
        });

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    [UseBodyValidator(Validator = typeof(RefreshRequestValidator))]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await authService.GetRefreshTokken(new RefreshTokkenParams
        {
            RefreshToken = request.RefreshToken,
            Email = request.Email
        });

        return Ok(new RefreshResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await authService.Logout(new LogoutParams
        {
            Email = request.Email,
            RefreshToken = request.RefreshToken
        });
        return Ok(new { message = "Logged out successfully." });
    }
}
