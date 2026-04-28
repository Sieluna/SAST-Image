using Domain.AlbumAggregate.ImageEntity;
using Mediator;

namespace Storage.Images.Messages;

internal sealed record class ImageDeletedMessage(DateTime Time, ImageId ImageId)
    : OutboxMessage(Time),
        IOutboxMessage
{
    public static string Type { get; } = "image_deleted";

    [Obsolete("ORM only", true)]
    public ImageDeletedMessage()
        : this(null!) { }
}

internal sealed class ImageDeletedMessageHandler(IImageFileManager manager)
    : IOutboxMessageHandler<ImageDeletedMessage>
{
    public async ValueTask<Unit> Handle(
        ImageDeletedMessage message,
        CancellationToken cancellationToken
    )
    {
        await manager.DeleteAsync(message.ImageId, cancellationToken);

        return Unit.Value;
    }
}
