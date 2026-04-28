using Domain.AlbumAggregate.ImageEntity;
using Domain.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Query.Images.Queries;

public sealed record class ImagesQuery(long? AuthorId, long? AlbumId, long? Cursor, Actor Actor)
    : IQuery<ImageDto[]>
{
    const int PageSize = 50;

    internal static readonly Func<
        QueryDbContext,
        long?,
        long?,
        long?,
        long,
        bool,
        bool,
        IAsyncEnumerable<ImageDto>
    > Query = EF.CompileAsyncQuery(
        (
            QueryDbContext context,
            long? authorId,
            long? albumId,
            long? cursor,
            long actorId,
            bool isAuthenticated,
            bool isAdmin
        ) =>
            context
                .Images.AsNoTracking()
                .Where(i => i.Status == ImageStatusValue.Available)
                .Where(i => authorId == null || i.UploaderId == authorId.Value)
                .Where(i => albumId == null || i.AlbumId == albumId.Value)
                .WhereIsAccessible(actorId, isAuthenticated, isAdmin)
                .OrderByDescending(i => i.Id)
                .SkipWhile(i => cursor != null && i.Id != cursor)
                .Take(PageSize)
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

internal sealed class ImagesQueryHandler(QueryDbContext context)
    : IQueryHandler<ImagesQuery, ImageDto[]>
{
    public async ValueTask<ImageDto[]> Handle(
        ImagesQuery request,
        CancellationToken cancellationToken
    )
    {
        return await ImagesQuery
            .Query(
                context,
                request.AuthorId,
                request.AlbumId,
                request.Cursor,
                request.Actor.Id.Value,
                request.Actor.IsAuthenticated,
                request.Actor.IsAdmin
            )
            .ToArrayAsync(cancellationToken);
    }
}
