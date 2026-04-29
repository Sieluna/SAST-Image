using Domain.Event;
using Domain.UserAggregate.Events;
using Query.Database;

namespace Query.Users.EventHandlers;

internal sealed class ProfileUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<ProfileUpdatedEvent>
{
    public async ValueTask Handle(ProfileUpdatedEvent e, CancellationToken cancellationToken)
    {
        var user = await context.Users.GetAsync(u => u.Id == e.Id.Value, cancellationToken);

        user.Biography = e.Biography.Value;
        user.Nickname = e.Nickname.Value;
    }
}
