using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Albums.EventHandlers;

internal sealed class AlbumDescriptionUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumDescriptionUpdatedEvent>
{
    public async ValueTask Handle(
        AlbumDescriptionUpdatedEvent e,
        CancellationToken cancellationToken
    )
    {
        var album = await context.Albums.GetAsync(
            album => album.Id == e.Album.Value,
            cancellationToken
        );

        album.Description = e.Description.Value;
    }
}
