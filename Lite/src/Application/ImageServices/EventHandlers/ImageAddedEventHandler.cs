using Domain.AlbumAggregate.Events;
using Domain.Core.Event;

namespace Application.ImageServices.EventHandlers;

internal sealed class ImageAddedEventHandler(
    IImageModelRepository repository,
    IImageStorageManager manager
) : IDomainEventHandler<ImageAddedEvent>
{
    public async ValueTask Handle(ImageAddedEvent e, CancellationToken cancellationToken)
    {
        ImageModel image = new(e);

        await manager.StoreImageAsync(e.ImageId, e.ImageFile.Stream, cancellationToken);

        await repository.AddAsync(image, cancellationToken);
    }
}
