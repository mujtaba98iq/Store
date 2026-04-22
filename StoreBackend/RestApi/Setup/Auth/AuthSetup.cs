using Domain.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RestApi.Setup;

public static class AuthSetup
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.ConfigureAuth();

        builder.Services.AddScoped<Domain.Auth.IAuthService, Domain.Auth.AuthService>();

        return builder;
    }
}
