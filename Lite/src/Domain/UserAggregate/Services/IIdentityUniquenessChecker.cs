using Domain.UserAggregate.UserEntity;

namespace Domain.UserAggregate.Services;

public interface IIdentityUniquenessChecker
{
    public Task CheckAsync(ExternalId identityId, CancellationToken cancellationToken);
}
