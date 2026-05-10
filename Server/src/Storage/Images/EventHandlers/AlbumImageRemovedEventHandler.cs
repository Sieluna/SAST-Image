using Domain.Album.Events;
using Domain.Event;
using Storage.Services;

namespace Storage.Images.EventHandlers;

public sealed class AlbumImageRemovedEventHandler(IImageFileManager manager)
    : IDomainEventHandler<AlbumImageRemovedEvent>
{
    public async ValueTask Handle(AlbumImageRemovedEvent e, CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(e.ImageId, cancellationToken);
    }
}
