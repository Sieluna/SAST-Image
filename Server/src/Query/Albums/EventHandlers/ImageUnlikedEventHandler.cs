using Domain.Album.Events;
using Domain.Event;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Query.Albums.EventHandlers;

public sealed class ImageUnlikedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ImageUnLikedEvent>
{
    public async ValueTask Handle(ImageUnLikedEvent e, CancellationToken cancellationToken)
    {
        await context
            .Likes.Where(like => like.User == e.Actor.Id)
            .Where(like => like.Image == e.ImageId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
