using Domain.Event;

namespace Domain.User.Events;

public sealed record class UsernameUpdatedEvent(UserId Id, Username Username) : DomainEventBase;
