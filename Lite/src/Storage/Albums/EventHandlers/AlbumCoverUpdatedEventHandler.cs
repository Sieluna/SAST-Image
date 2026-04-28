using Domain.AlbumAggregate.Events;
using Domain.Event;

namespace Storage.Albums.EventHandlers;

internal sealed class AlbumCoverUpdatedEventHandler(
    IImageFileManager manager,
    ICompressProcessor compressor
) : IDomainEventHandler<AlbumCoverUpdatedEvent>
{
    public async ValueTask Handle(AlbumCoverUpdatedEvent e, CancellationToken cancellationToken)
    {
        var task = e switch
        {
            AlbumCoverUpdatedManuallyEvent { Album: var album, File: var file } =>
                manager.SaveAsync(file, album, cancellationToken),
            AlbumCoverUpdatedEmptyEvent { Album: var album } => manager.DeleteAsync(
                album,
                cancellationToken
            ),
            AlbumCoverUpdatedAutomaticallyEvent { Album: var album, Image: var image }
                when manager.TryGet(image, out var file) => manager.SaveAsync(
                file.Value,
                album,
                cancellationToken
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported event type: {e.GetType().FullName}"
            ),
        };

        await task;
        await compressor.CompressAsync(e.Album, cancellationToken);
    }
}
