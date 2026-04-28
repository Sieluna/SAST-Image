using Domain.AlbumAggregate.ImageEntity;
using Domain.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Query.Images.Queries;

public sealed class DetailedImage
{
    public required long Id { get; init; }
    public required long AlbumId { get; init; }
    public required long UploaderId { get; init; }
    public required string Title { get; init; }
    public required DateTime UploadedAt { get; init; }
    public required string[] Tags { get; init; }
    public required int Likes { get; init; }
    public required RequesterInfo Requester { get; init; }

    public readonly record struct RequesterInfo(bool Liked);
}

public sealed record DetailedImageQuery(ImageId Image, Actor Actor) : IQuery<DetailedImage?>
{
    internal static readonly Func<
        QueryDbContext,
        long,
        long,
        bool,
        bool,
        Task<DetailedImage?>
    > Query = EF.CompileAsyncQuery(
        (QueryDbContext context, long imageId, long actorId, bool isAuthenticated, bool isAdmin) =>
            context
                .Images.AsNoTracking()
                .Where(i => i.Status == ImageStatusValue.Available)
                .Where(i => i.Id == imageId)
                .WhereIsAccessible(actorId, isAuthenticated, isAdmin)
                .Select(i => new DetailedImage
                {
                    Id = i.Id,
                    AlbumId = i.AlbumId,
                    UploaderId = i.UploaderId,
                    Title = i.Title,
                    UploadedAt = i.UploadedAt,
                    Tags = i.Tags,
                    Likes = i.Likes.Count,
                    Requester = new(i.Likes.Select(l => l.User).Contains(actorId)),
                })
                .FirstOrDefault()
    );
}

internal sealed class DetailedImageQueryHandler(QueryDbContext context)
    : IQueryHandler<DetailedImageQuery, DetailedImage?>
{
    public async ValueTask<DetailedImage?> Handle(
        DetailedImageQuery request,
        CancellationToken cancellationToken
    )
    {
        return await DetailedImageQuery.Query(
            context,
            request.Image.Value,
            request.Actor.Id.Value,
            request.Actor.IsAuthenticated,
            request.Actor.IsAdmin
        );
    }
}
