using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RestApi.Extensions;

public static class ControllerExtensions
{
    public static string? GetNullableUserId(this ControllerBase controller)
    {
        return controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// The token generator always writes the user id into the identifier claim, so it can
    /// be read back as the key of the rows the caller owns.
    /// </summary>
    public static Guid GetUserGuid(this ControllerBase controller)
    {
        return Guid.Parse(GetUserId(controller));
    }

    public static string GetUserId(this ControllerBase controller)
    {
        return GetNullableUserId(controller)!;
    }
}
