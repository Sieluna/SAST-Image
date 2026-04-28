using Domain.AlbumAggregate.Events;
using Domain.Event;
using Query.Database;

namespace Query.Images.EventHandlers;

internal sealed class ImageTagUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ImageTagsUpdatedEvent>
{
    public async ValueTask Handle(ImageTagsUpdatedEvent e, CancellationToken cancellationToken)
    {
        var image = await context.Images.GetAsync(
            image => image.Id == e.Id.Value,
            cancellationToken
        );

        image.Tags = e.Tags.Value;
    }
}
