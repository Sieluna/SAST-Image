using Domain.Event;
using Domain.UserAggregate.Events;
using Query.Database;

namespace Query.Users.EventHandlers;

internal sealed class BiographyUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<BiographyUpdatedEvent>
{
    public async ValueTask Handle(BiographyUpdatedEvent e, CancellationToken cancellationToken)
    {
        var user = await context.Users.GetAsync(u => u.Id == e.User.Value, cancellationToken);

        user.Biography = e.Biography.Value;
    }
}
