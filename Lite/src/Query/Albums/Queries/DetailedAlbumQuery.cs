using Domain.AlbumAggregate.AlbumEntity;
using Domain.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Query.Albums.Queries;

public sealed class DetailedAlbum
{
    public required long Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required long Author { get; init; }
    public required long Category { get; init; }
    public required string[] Tags { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required AccessLevelValue AccessLevel { get; init; }
    public required int SubscribeCount { get; init; }
}

public sealed record class DetailedAlbumQuery(AlbumId Id, Actor Actor) : IQuery<DetailedAlbum?>
{
    public static readonly Func<
        QueryDbContext,
        long,
        long,
        bool,
        bool,
        Task<DetailedAlbum?>
    > Query = EF.CompileAsyncQuery(
        (QueryDbContext context, long id, long actorId, bool isAuthenticated, bool isAdmin) =>
            context
                .Albums.AsNoTracking()
                .Where(a => a.Id == id)
                .Where(a => a.RemovedAt == null)
                .WhereIsAccessible(actorId, isAuthenticated, isAdmin)
                .Select(a => new DetailedAlbum
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Author = a.AuthorId,
                    Category = a.CategoryId,
                    Tags = a.Tags,
                    UpdatedAt = a.UpdatedAt,
                    CreatedAt = a.CreatedAt,
                    AccessLevel = a.AccessLevel,
                    SubscribeCount = a.Subscribes.Count,
                })
                .FirstOrDefault()
    );
}

public sealed class DetailedAlbumQueryHandler(QueryDbContext context)
    : IQueryHandler<DetailedAlbumQuery, DetailedAlbum?>
{
    public async ValueTask<DetailedAlbum?> Handle(
        DetailedAlbumQuery request,
        CancellationToken cancellationToken
    )
    {
        return await DetailedAlbumQuery.Query(
            context,
            request.Id.Value,
            request.Actor.Id,
            request.Actor.IsAuthenticated,
            request.Actor.IsAdmin
        );
    }
}
