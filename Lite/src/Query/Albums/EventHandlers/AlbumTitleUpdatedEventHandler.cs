using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Albums.EventHandlers;

internal sealed class AlbumTitleUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumTitleUpdatedEvent>
{
    public async ValueTask Handle(AlbumTitleUpdatedEvent e, CancellationToken cancellationToken)
    {
        var album = await context.Albums.GetAsync(
            album => album.Id == e.Album.Value,
            cancellationToken
        );

        album.Title = e.Title.Value;
    }
}
