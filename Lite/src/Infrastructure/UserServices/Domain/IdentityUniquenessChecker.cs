using Domain.UserAggregate.Exceptions;
using Domain.UserAggregate.IdentityEntity;
using Domain.UserAggregate.Services;
using Infrastructure.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UserServices.Domain;

internal sealed class IdentityUniquenessChecker(DomainDbContext context)
    : IIdentityUniquenessChecker
{
    public async Task CheckAsync(
        IdentityId identityId,
        IdentityProvider provider,
        CancellationToken cancellationToken
    )
    {
        bool isExisting = await context
            .Users.FromSql(
                $"Select 1 From domain.identities WHERE provider_user_id = {identityId.Value} AND provider = {provider}"
            )
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (isExisting)
            throw new IdentityDuplicateException(identityId);
    }
}
