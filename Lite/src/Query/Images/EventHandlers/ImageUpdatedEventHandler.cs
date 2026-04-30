using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Images.EventHandlers;

internal sealed class ImageUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ImageInfoUpdatedEvent>
{
    public async ValueTask Handle(ImageInfoUpdatedEvent e, CancellationToken cancellationToken)
    {
        var image = await context.Images.GetAsync(
            image => image.Id == e.Id.Value,
            cancellationToken
        );

        if (e.Title is { Value: { } title })
            image.Title = title;
        if (e.Tags is { Value: { } tags })
            image.Tags = tags;
    }
}
