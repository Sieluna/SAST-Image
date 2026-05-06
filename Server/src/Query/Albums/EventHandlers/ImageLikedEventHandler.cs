using Domain.Album.Events;
using Domain.Event;
using Query.Database;
using Query.Images;

namespace Query.Albums.EventHandlers;

public sealed class ImageLikedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ImageLikedEvent>
{
    public async ValueTask Handle(ImageLikedEvent e, CancellationToken cancellationToken)
    {
        LikeModel like = new(e.ImageId, e.Actor.Id);

        await context.Likes.AddAsync(like, cancellationToken);
    }
}
