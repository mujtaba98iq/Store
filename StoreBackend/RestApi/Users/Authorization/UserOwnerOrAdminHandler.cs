
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RestApi.Users.Authorization;

public class UserOwnerOrAdminHandler : AuthorizationHandler<UserOwnerOrAdminRequirement,Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserOwnerOrAdminRequirement requirement, Guid userId)
    {
       if (context.User.IsInRole("Admin"))
       {
           context.Succeed(requirement);
            return Task.CompletedTask;
       }

       var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(currentUserId.ToString() == userId.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;  
    }
}
