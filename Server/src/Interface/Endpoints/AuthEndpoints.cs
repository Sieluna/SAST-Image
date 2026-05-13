using Domain;
using Domain.Api;
using Domain.User;
using Interface.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Query.Database;
using Query.User;
using UserGrain = Domain.User.IUserGrain;

namespace Interface.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("/login", async (
            LoginRequest request,
            IMediator mediator,
            IDbContextFactory<QueryDbContext> queryDb,
            JwtTokenService jwt) =>
        {
            var existence = await mediator.Send(
                new UsernameExistenceQuery(new Username(request.Username)));
            if (!existence.IsExist)
                return Results.NotFound(new ErrorResponse("User not found", 404));

            await using var db = await queryDb.CreateDbContextAsync();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user is null)
                return Results.NotFound(new ErrorResponse("User not found", 404));

            var token = jwt.Generate(new UserId(user.Id), new Username(user.Username), Role.User);
            return Results.Ok(new JwtTokenResponse(token.AccessToken, token.RefreshToken, token.ExpireIn));
        });

        group.MapPost("/register", async (
            RegisterRequest request,
            IGrainFactory grains,
            JwtTokenService jwt) =>
        {
            AuthHelper.SetActor(null);
            var grain = grains.GetGrain<IUserGrain>(0);
            var userId = await grain.Register(
                new Username(request.Username),
                new Nickname(request.Nickname),
                new Biography(request.Biography));

            var token = jwt.Generate(userId, new Username(request.Username), Role.User);
            return Results.Ok(new JwtTokenResponse(token.AccessToken, token.RefreshToken, token.ExpireIn));
        });

        group.MapPost("/refresh", (
            RefreshTokenRequest request,
            JwtTokenService jwt) =>
        {
            var (userId, isValid) = jwt.DecodeRefreshToken(request.RefreshToken);
            if (!isValid)
                return Results.Unauthorized();

            var token = jwt.Generate(userId, new Username("unknown"), Role.User);
            return Results.Ok(new JwtTokenResponse(token.AccessToken, token.RefreshToken, token.ExpireIn));
        });
    }
}
