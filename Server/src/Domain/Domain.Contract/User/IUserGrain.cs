using Domain.Filters;
using Orleans.Concurrency;

namespace Domain.User;

[Alias("UserGrain")]
public interface IUserGrain : IGrainWithIntegerKey
{
    [EnsureUniqueId]
    [AllowRecordNotExists]
    [Alias(nameof(Register))]
    ValueTask<UserId> Register(Username username, Nickname Nickname, Biography Biography);

    [AccessControl]
    [Alias(nameof(UpdateProfile))]
    ValueTask UpdateProfile(Username? username, Nickname? nickname, Biography? biography);

    [AccessControl]
    [Alias(nameof(UpdateAvatar))]
    ValueTask UpdateAvatar(Immutable<byte[]> file, CancellationToken cancellationToken = default);

    [AccessControl]
    [Alias(nameof(UpdateHeader))]
    ValueTask UpdateHeader(Immutable<byte[]> file, CancellationToken cancellationToken = default);
}
