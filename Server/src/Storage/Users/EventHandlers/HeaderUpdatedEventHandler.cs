using Domain.Event;
using Domain.File;
using Domain.User.Events;
using Storage.Services;

namespace Storage.Users.EventHandlers;

public sealed class HeaderUpdatedEventHandler(
    IImageFileManager manager,
    ICompressProcessor compressor,
    IGrainFactory factory
) : IDomainEventHandler<HeaderUpdatedEvent>
{
    public async ValueTask Handle(HeaderUpdatedEvent e, CancellationToken cancellationToken)
    {
        var domainManager = factory.GetGrain<IFileManagerGrain>(Guid.Empty);

        await manager.SaveAsync(
            domainManager.GetFileAsync(e.File, cancellationToken),
            e.Id,
            "header",
            cancellationToken
        );

        await compressor.CompressAsync(e.Id, "header", cancellationToken);
    }
}
