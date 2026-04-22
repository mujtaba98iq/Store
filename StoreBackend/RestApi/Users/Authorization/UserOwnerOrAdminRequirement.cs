using Microsoft.AspNetCore.Authorization;

namespace RestApi.Users.Authorization;

public class UserOwnerOrAdminRequirement : IAuthorizationRequirement
{
}
