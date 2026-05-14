using Domain.Event;
using Domain.User.Events;
using Mediator;
using Query.Database;

namespace Query.User.EventHandlers;

internal class UsernameUpdatedEventHandler(QueryDbContext context)
    : IDomainEventHandler<UsernameUpdatedEvent>
{
    public async ValueTask<Unit> Handle(UsernameUpdatedEvent e, CancellationToken cancellationToken)
    {
        var user = await context.Users.GetAsync(m => m.Id == e.Id, cancellationToken);

        user.Username = e.Username.Value;

        return Unit.Value;
    }
}
