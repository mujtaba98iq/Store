using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RestApi.Extensions;

public static class ControllerExtensions
{
    public static string? GetNullableUserId(this ControllerBase controller)
    {
        return controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string GetUserId(this ControllerBase controller)
    {
        return GetNullableUserId(controller)!;
    }
}
