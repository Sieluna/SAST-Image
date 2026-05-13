using Domain;
using Domain.Album;
using Domain.Api;
using Domain.Category;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Query.Album;
using Query.Database;
using AlbumGrain = Domain.Album.IAlbumGrain;
using AlbumId = Domain.Album.AlbumId;
using AlbumTitle = Domain.Album.AlbumTitle;
using AlbumDescription = Domain.Album.AlbumDescription;
using AlbumTags = Domain.Album.AlbumTags;

namespace Interface.Endpoints;

internal static class AlbumEndpoints
{
    public static void MapAlbumEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/albums").RequireAuthorization();

        group.MapGet("/", async (
            long? categoryId,
            long? cursor,
            HttpContext context,
            IMediator mediator,
            IDbContextFactory<QueryDbContext> queryDb) =>
        {
            var actor = context.GetActor();
            var albums = await mediator.Send(new AlbumsQuery(categoryId, null, null, cursor, actor));
            var responses = await ToAlbumResponses(albums, queryDb);
            return Results.Ok(new AlbumListResponse(responses, null));
        });

        group.MapGet("/{id:long}", async (
            long id,
            HttpContext context,
            IMediator mediator,
            IDbContextFactory<QueryDbContext> queryDb) =>
        {
            var actor = context.GetActor();
            var albums = await mediator.Send(new AlbumsQuery(null, null, null, null, actor));
            var album = albums.FirstOrDefault(a => a.Id == id);
            if (album is null)
                return Results.NotFound(new ErrorResponse("Album not found", 404));

            var responses = await ToAlbumResponses([album], queryDb);
            return Results.Ok(responses[0]);
        });

        group.MapPost("/", async (
            CreateAlbumRequest request,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(0);
            var albumId = await grain.Create(
                new AlbumTitle(request.Title),
                new AlbumDescription(request.Description),
                new AlbumTags(request.Tags),
                new CategoryId(request.CategoryId));

            return Results.Ok(new AlbumResponse(
                albumId.Value, request.Title, request.Description,
                actor.Id.Value, "unknown", request.CategoryId, "unknown",
                request.Tags, 0, AccessLevel.PublicReadWrite,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        });

        group.MapPut("/{id:long}", async (
            long id,
            UpdateAlbumRequest request,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(id);
            await grain.Update(
                request.Title is not null ? new AlbumTitle(request.Title) : null,
                request.Description is not null ? new AlbumDescription(request.Description) : null,
                request.Tags is not null ? new AlbumTags(request.Tags) : null,
                request.CategoryId is not null ? new CategoryId(request.CategoryId.Value) : null);
            return Results.Ok();
        });

        group.MapDelete("/{id:long}", async (
            long id,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(id);
            await grain.Remove();
            return Results.Ok();
        });

        group.MapPost("/{id:long}/subscribe", async (
            long id,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(id);
            await grain.Subscribe();
            return Results.Ok();
        });

        group.MapDelete("/{id:long}/subscribe", async (
            long id,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(id);
            await grain.Unsubscribe();
            return Results.Ok();
        });
    }

    private static async Task<AlbumResponse[]> ToAlbumResponses(
        AlbumDto[] albums,
        IDbContextFactory<QueryDbContext> queryDb)
    {
        if (albums.Length == 0) return [];

        await using var db = await queryDb.CreateDbContextAsync();
        var userIds = albums.Select(a => a.Author).Distinct().ToArray();
        var categoryIds = albums.Select(a => a.Category).Distinct().ToArray();

        var users = await db.Users.Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);
        var categories = await db.Categories.Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name);

        return albums.Select(a => new AlbumResponse(
            a.Id, a.Title, a.Description,
            a.Author, users.GetValueOrDefault(a.Author, "unknown"),
            a.Category, categories.GetValueOrDefault(a.Category, "unknown"),
            a.Tags, a.SubscribeCount, AccessLevel.PublicReadWrite,
            new DateTimeOffset(a.CreatedAt).ToUnixTimeSeconds(),
            new DateTimeOffset(a.UpdatedAt).ToUnixTimeSeconds()
        )).ToArray();
    }
}
