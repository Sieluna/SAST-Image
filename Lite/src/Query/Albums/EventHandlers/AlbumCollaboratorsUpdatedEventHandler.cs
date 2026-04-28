using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Albums.EventHandlers;

internal sealed class AlbumCollaboratorsUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<AlbumCollaboratorsUpdatedEvent>
{
    public async ValueTask Handle(
        AlbumCollaboratorsUpdatedEvent e,
        CancellationToken cancellationToken
    )
    {
        var album = await context.Albums.GetAsync(
            album => album.Id == e.Album.Value,
            cancellationToken
        );

        album.Collaborators = Array.ConvertAll(e.Collaborators.Value, id => id.Value);
    }
}
