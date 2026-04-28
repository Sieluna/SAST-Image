using Domain.AlbumAggregate.AlbumEntity;
using Domain.AlbumAggregate.ImageEntity;
using Domain.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Query.Images.Queries;

public sealed record RemovedImagesQuery(AlbumId Album, Actor Actor) : IQuery<ImageDto[]>
{
    internal static readonly Func<
        QueryDbContext,
        long,
        long,
        bool,
        IAsyncEnumerable<ImageDto>
    > Query = EF.CompileAsyncQuery(
        (QueryDbContext context, long albumId, long actorId, bool isAdmin) =>
            context
                .Images.AsNoTracking()
                .Where(i => i.Status == ImageStatusValue.Removed)
                .Where(i => i.AlbumId == albumId)
                .Where(i => i.AuthorId == actorId || i.Collaborators.Contains(actorId) || isAdmin)
                .Select(i => new ImageDto(
                    i.Id,
                    i.UploaderId,
                    i.AlbumId,
                    i.Title,
                    i.Tags,
                    i.UploadedAt,
                    i.RemovedAt
                ))
    );
}

internal sealed class RemovedImagesQueryHandler(QueryDbContext context)
    : IQueryHandler<RemovedImagesQuery, ImageDto[]>
{
    public async ValueTask<ImageDto[]> Handle(
        RemovedImagesQuery request,
        CancellationToken cancellationToken
    )
    {
        return await RemovedImagesQuery
            .Query(context, request.Album.Value, request.Actor.Id, request.Actor.IsAdmin)
            .ToArrayAsync(cancellationToken);
    }
}
