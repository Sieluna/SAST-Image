using Domain.Album.Events;
using Domain.Event;
using Mediator;
using Query.Database;

namespace Query.Album.EventHandlers;

public sealed class AlbumUnsubscribedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumUnsubscribedEvent>
{
    public async ValueTask<Unit> Handle(
        AlbumUnsubscribedEvent e,
        CancellationToken cancellationToken
    )
    {
        var album = await context.Albums.GetAsync(a => a.Id == e.Id, cancellationToken);

        album.Subscribes.Remove(e.Actor.Id.Value);

        return Unit.Value;
    }
}
