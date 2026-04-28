using Domain.Event;
using Domain.UserAggregate.Events;
using Query.Database;

namespace Query.Users.EventHandlers;

internal sealed class NicknameUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<NicknameUpdatedEvent>
{
    public async ValueTask Handle(NicknameUpdatedEvent e, CancellationToken cancellationToken)
    {
        var user = await context.Users.GetAsync(u => u.Id == e.Id.Value, cancellationToken);

        user.Nickname = e.Nickname.Value;
    }
}
