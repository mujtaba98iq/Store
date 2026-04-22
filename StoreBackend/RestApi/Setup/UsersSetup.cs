using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using RestApi.Users;
using RestApi.Users.Authorization;

namespace RestApi.Setup;

public static class UsersSetup
{
    public static WebApplicationBuilder AddUsersModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserResponseFormatter, UserResponseFormatter>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserOwnerOrAdminHandler>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("UserOwnerOrAdminPolicy", policy =>
                policy.Requirements.Add(new UserOwnerOrAdminRequirement()));
        });

        return builder;
    }
}
