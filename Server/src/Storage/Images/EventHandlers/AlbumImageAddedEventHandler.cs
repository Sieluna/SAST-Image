using Domain.Album.Events;
using Domain.Event;
using Domain.File;
using Storage.Services;

namespace Storage.Images.EventHandlers;

public sealed class AlbumImageAddedEventHandler(
    IImageFileManager manager,
    ICompressProcessor compressor,
    IGrainFactory factory
) : IDomainEventHandler<AlbumImageAddedEvent>
{
    public async ValueTask Handle(AlbumImageAddedEvent e, CancellationToken cancellationToken)
    {
        var domainManager = factory.GetGrain<IFileManagerGrain>(Guid.Empty);

        await manager.SaveAsync(
            domainManager.GetFileAsync(e.File, cancellationToken),
            e.Id,
            cancellationToken
        );

        await compressor.CompressAsync(e.Id, null, "compressed", cancellationToken);
    }
}
