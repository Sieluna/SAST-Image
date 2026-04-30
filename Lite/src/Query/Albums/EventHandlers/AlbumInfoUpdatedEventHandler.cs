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

        if (e.Title.HasValue)
            album.Title = e.Title.Value.Value;
        if (e.Description.HasValue)
            album.Description = e.Description.Value.Value;
        if (e.Tags is not null)
            album.Tags = e.Tags.Value;
    }
}
