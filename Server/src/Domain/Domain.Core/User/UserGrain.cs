using Domain.Event;
using Domain.File;
using Domain.Filters;
using Domain.User.Events;
using Orleans.Concurrency;

namespace Domain.User;

internal sealed class UserGrain : DomainGrain<UserState>, IUserGrain, IIncomingGrainCallFilter
{
    public Task Invoke(IIncomingGrainCallContext context)
    {
        if (context.MethodName == nameof(Register))
            return context.Invoke();
        if (Id != Actor.Id)
            throw new ForbiddenException();
        return context.Invoke();
    }

    public async ValueTask<UserId?> Register( // TODO: Change to `Union` in .NET11
        Username username,
        Nickname nickname,
        Biography biography,
        CancellationToken cancellationToken = default
    )
    {
        var manager = GrainFactory.GetGrain<IUsernameManagerGrain>(Guid.Empty);

        if (await manager.Put(Id, username, cancellationToken) is false)
            return null;
        RaiseEvent(new UserRegisteredEvent(Id, username, nickname, biography));
        return Id;
    }

    public async ValueTask UpdateAvatar(
        Immutable<byte[]> file,
        CancellationToken cancellationToken = default
    )
    {
        var manager = GrainFactory.GetGrain<IFileSyncGrain>(Guid.Empty);
        var key = await manager.UploadAsync(file, cancellationToken);
        RaiseEvent(new AvatarUpdatedEvent(Id, key));
    }

    public async ValueTask<bool> UpdateUsername(
        Username username,
        CancellationToken cancellationToken = default
    )
    {
        var manager = GrainFactory.GetGrain<IUsernameManagerGrain>(Guid.Empty);

        if (await manager.Put(Id, username, cancellationToken) is false)
            return false;

        RaiseEvent(new UsernameUpdatedEvent(Id, username));
        return true;
    }

    public async ValueTask UpdateHeader(
        Immutable<byte[]> file,
        CancellationToken cancellationToken = default
    )
    {
        var manager = GrainFactory.GetGrain<IFileSyncGrain>(Guid.Empty);
        var key = await manager.UploadAsync(file, cancellationToken);
        RaiseEvent(new HeaderUpdatedEvent(Id, key));
    }

    public async ValueTask UpdateProfile(
        Nickname? nickname,
        Biography? biography,
        CancellationToken cancellationToken = default
    )
    {
        RaiseEvent(new ProfileUpdatedEvent(Id, nickname, biography));
    }
}

internal sealed class UserState
    : DomainStateBase,
        IDomainEventApplicable<UserRegisteredEvent>,
        IDomainEventApplicable<ProfileUpdatedEvent>,
        IDomainEventApplicable<AvatarUpdatedEvent>,
        IDomainEventApplicable<HeaderUpdatedEvent>
{
    public override void Apply(DomainEventBase e)
    {
        switch (e)
        {
            case UserRegisteredEvent e1:
                Apply(e1);
                break;
            case ProfileUpdatedEvent e2:
                Apply(e2);
                break;
            case AvatarUpdatedEvent e3:
                Apply(e3);
                break;
            case HeaderUpdatedEvent e4:
                Apply(e4);
                break;
            default:
                throw new NotSupportedException(
                    $"Event type {e.GetType().FullName} is not supported."
                );
        }
    }

    public void Apply(ProfileUpdatedEvent e)
    {
        // Do nothing.
    }

    public void Apply(UserRegisteredEvent e)
    {
        RecordExists = true;
    }

    public void Apply(AvatarUpdatedEvent e)
    {
        // Do nothing.
    }

    public void Apply(HeaderUpdatedEvent e)
    {
        // Do nothing.
    }
}
