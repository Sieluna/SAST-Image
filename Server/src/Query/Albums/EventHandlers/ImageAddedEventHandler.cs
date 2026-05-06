using Domain.Album.Events;
using Domain.Event;
using Query.Database;
using Query.Images;

namespace Query.Albums.EventHandlers;

public sealed class ImageAddedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ImageAddedEvent>
{
    public async ValueTask Handle(ImageAddedEvent e, CancellationToken cancellationToken)
    {
        var album = await context.Albums.GetAsync(album => album.Id == e.Id, cancellationToken);

        album.Images.Add(new ImageModel(e));

        album.UpdatedAt = e.Timestamp;
    }
}
