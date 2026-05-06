using Domain.Album.Events;
using Domain.Event;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Query.Albums.EventHandlers;

public sealed class ImageRemovedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ImageRemovedEvent>
{
    public async ValueTask Handle(ImageRemovedEvent e, CancellationToken cancellationToken)
    {
        await context.Images.Where(i => i.Id == e.ImageId).ExecuteDeleteAsync(cancellationToken);
    }
}
