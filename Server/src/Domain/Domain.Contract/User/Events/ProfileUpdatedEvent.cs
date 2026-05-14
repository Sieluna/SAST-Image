using Domain.Event;

namespace Domain.User.Events;

public sealed record class ProfileUpdatedEvent(UserId Id, Nickname? Nickname, Biography? Biography)
    : DomainEventBase { }
