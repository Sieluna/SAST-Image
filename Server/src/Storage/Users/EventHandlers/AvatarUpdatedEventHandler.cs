using Domain.Event;
using Domain.File;
using Domain.User.Events;
using Storage.Services;

namespace Storage.Users.EventHandlers;

public sealed class AvatarUpdatedEventHandler(
    IImageFileManager manager,
    ICompressProcessor compressor,
    IGrainFactory factory
) : IDomainEventHandler<AvatarUpdatedEvent>
{
    public async ValueTask Handle(AvatarUpdatedEvent e, CancellationToken cancellationToken)
    {
        var domainManager = factory.GetGrain<IFileManagerGrain>(Guid.Empty);

        await manager.SaveAsync(
            domainManager.GetFileAsync(e.File, cancellationToken),
            e.Id,
            "avatar",
            cancellationToken
        );

        await compressor.CompressAsync(e.Id, "avatar", cancellationToken);
    }
}
