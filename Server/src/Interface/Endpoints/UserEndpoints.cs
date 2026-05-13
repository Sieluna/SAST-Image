using Domain.Api;
using Domain.User;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;
using Query.Database;
using Query.User;
using UserGrain = Domain.User.IUserGrain;
using Username = Domain.User.Username;
using Nickname = Domain.User.Nickname;
using Biography = Domain.User.Biography;

namespace Interface.Endpoints;

internal static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").RequireAuthorization();

        group.MapGet("/profile", async (
            long? userId,
            HttpContext context,
            IMediator mediator,
            IDbContextFactory<QueryDbContext> queryDb) =>
        {
            var actor = context.GetActor();
            var id = userId.HasValue ? userId.Value : actor.Id.Value;
            var dto = await mediator.Send(new UserProfileQuery(id));

            if (dto is null)
                return Results.NotFound(new ErrorResponse("User not found", 404));

            await using var db = await queryDb.CreateDbContextAsync();
            var registeredAt = await db.Users
                .Where(u => u.Id == dto.Id)
                .Select(u => u.RegisteredAt)
                .FirstOrDefaultAsync();

            return Results.Ok(new UserProfileResponse(
                dto.Id, dto.Username, dto.Nickname, dto.Biography,
                new DateTimeOffset(registeredAt).ToUnixTimeSeconds()));
        });

        group.MapPut("/profile", async (
            UpdateProfileRequest request,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IUserGrain>(actor.Id.Value);
            await grain.UpdateProfile(
                request.Username is not null ? new Username(request.Username) : null,
                request.Nickname is not null ? new Nickname(request.Nickname) : null,
                request.Biography is not null ? new Biography(request.Biography) : null);
            return Results.Ok();
        });

        group.MapPut("/avatar", async (
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IUserGrain>(actor.Id.Value);
            await grain.UpdateAvatar(fileBytes.AsImmutable());
            return Results.Ok();
        });

        group.MapPut("/header", async (
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IUserGrain>(actor.Id.Value);
            await grain.UpdateHeader(fileBytes.AsImmutable());
            return Results.Ok();
        });
    }
}
