using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Albums.EventHandlers;

internal sealed class AlbumTagsUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumTagsUpdatedEvent>
{
    public async ValueTask Handle(AlbumTagsUpdatedEvent e, CancellationToken cancellationToken)
    {
        var album = await context.Albums.GetAsync(
            album => album.Id == e.Id.Value,
            cancellationToken
        );

        album.Tags = e.Tags.Value;
    }
}
