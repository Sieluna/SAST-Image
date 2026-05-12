using System.Security.Claims;
using Domain;
using Domain.User;

namespace Interface.Endpoints;

internal static class AuthHelper
{
    public static Actor GetActor(this HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("Not authenticated");

        var userIdClaim = user.FindFirst("id")?.Value;
        var roleClaim = user.FindFirst("role")?.Value;

        if (userIdClaim is null || !long.TryParse(userIdClaim, out var uid))
            throw new UnauthorizedAccessException("Invalid token: missing user id");

        return new Actor
        {
            Id = new UserId(uid),
            IsAuthenticated = true,
            Role = roleClaim == ((byte)Role.Admin).ToString() ? Role.Admin : Role.User,
        };
    }

    public static Actor TryGetActor(this HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst("id")?.Value;
            var roleClaim = user.FindFirst("role")?.Value;

            if (userIdClaim is not null && long.TryParse(userIdClaim, out var uid))
            {
                return new Actor
                {
                    Id = new UserId(uid),
                    IsAuthenticated = true,
                    Role = roleClaim == ((byte)Role.Admin).ToString() ? Role.Admin : Role.User,
                };
            }
        }

        return new Actor { Id = new UserId(0), IsAuthenticated = false, Role = Role.None };
    }

    public static void SetActor(Actor? actor)
    {
        Orleans.Runtime.RequestContext.Set("Actor", actor ?? new Actor { Id = new UserId(0), IsAuthenticated = false, Role = Role.None });
    }
}
