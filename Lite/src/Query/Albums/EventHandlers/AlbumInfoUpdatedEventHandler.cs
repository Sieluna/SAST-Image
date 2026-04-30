using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Albums.EventHandlers;

internal sealed class AlbumInfoUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumInfoUpdatedEvent>
{
    public async ValueTask Handle(AlbumInfoUpdatedEvent e, CancellationToken cancellationToken)
    {
        var album = await context.Albums.GetAsync(
            album => album.Id == e.Id.Value,
            cancellationToken
        );

        if (e.Title is { Value: { } title })
            album.Title = title;
        if (e.Description is { Value: { } description })
            album.Description = description;
        if (e.Tags is { Value: { } tags })
            album.Tags = tags;
    }
}
