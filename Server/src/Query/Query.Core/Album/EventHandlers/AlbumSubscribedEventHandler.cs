using Domain.Album.Events;
using Domain.Event;
using Mediator;
using Query.Database;

namespace Query.Album.EventHandlers;

public sealed class AlbumSubscribedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumSubscribedEvent>
{
    public async ValueTask<Unit> Handle(AlbumSubscribedEvent e, CancellationToken cancellationToken)
    {
        var album = await context.Albums.GetAsync(a => a.Id == e.Id, cancellationToken);

        album.Subscribes.Add(e.Actor.Id.Value);

        return Unit.Value;
    }
}
