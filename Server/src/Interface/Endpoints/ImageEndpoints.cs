using Domain.Album;
using Domain.Album.Image;
using Domain.Api;
using Domain.File;
using Mediator;
using Orleans.Concurrency;
using Query.Image;
using AlbumGrain = Domain.Album.IAlbumGrain;

namespace Interface.Endpoints;

internal static class ImageEndpoints
{
    public static void MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/albums/{albumId:long}/images").RequireAuthorization();

        group.MapGet("/", async (
            long albumId,
            long? cursor,
            HttpContext context,
            IMediator mediator) =>
        {
            var actor = context.GetActor();
            var images = await mediator.Send(new ImagesQuery(null, albumId, cursor, actor));
            var responses = images.Select(i => new ImageResponse(
                i.Id, i.AlbumId, i.Title, i.UploaderId, "unknown",
                i.Tags, i.Likes, i.Requester.Liked,
                new DateTimeOffset(i.UploadedAt).ToUnixTimeSeconds(),
                $"/images/{i.Id}")).ToArray();
            return Results.Ok(responses);
        });

        group.MapPost("/", async (
            long albumId,
            AddImageRequest request,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);

            var imageId = ImageId.GenerateNew();

            var fileManager = grains.GetGrain<IFileSyncGrain>(Guid.Empty);
            var fileKey = await fileManager.UploadAsync(request.FileBytes.AsImmutable(), CancellationToken.None);

            var grain = grains.GetGrain<IAlbumGrain>(albumId);
            await grain.AddImage(
                imageId,
                new ImageTitle(request.Title),
                new ImageTags(request.Tags),
                fileKey);

            return Results.Ok(new ImageResponse(
                imageId.Value, albumId, request.Title, actor.Id.Value, "unknown",
                request.Tags, 0, false,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                $"/images/{imageId.Value}"));
        });

        group.MapDelete("/{imageId:long}", async (
            long albumId,
            long imageId,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(albumId);
            await grain.RemoveImage(new ImageId(imageId));
            return Results.Ok();
        });

        group.MapPost("/{imageId:long}/like", async (
            long albumId,
            long imageId,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(albumId);
            await grain.LikeImage(new ImageId(imageId));
            return Results.Ok();
        });

        group.MapDelete("/{imageId:long}/like", async (
            long albumId,
            long imageId,
            HttpContext context,
            IGrainFactory grains) =>
        {
            var actor = context.GetActor();
            AuthHelper.SetActor(actor);
            var grain = grains.GetGrain<IAlbumGrain>(albumId);
            await grain.UnLikeImage(new ImageId(imageId));
            return Results.Ok();
        });
    }
}
