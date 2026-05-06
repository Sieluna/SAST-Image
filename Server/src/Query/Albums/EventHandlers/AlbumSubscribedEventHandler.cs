using Domain.Album.Events;
using Domain.Event;
using Query.Database;

namespace Query.Albums.EventHandlers;

public sealed class AlbumSubscribedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumSubscribedEvent>
{
    public async ValueTask Handle(AlbumSubscribedEvent e, CancellationToken cancellationToken)
    {
        await context.Subscribes.AddAsync(new SubscribeModel(e.Id, e.Actor.Id), cancellationToken);
    }
}
