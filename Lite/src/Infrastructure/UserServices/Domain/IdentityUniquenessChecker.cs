using Domain.UserAggregate.Exceptions;
using Domain.UserAggregate.Services;
using Domain.UserAggregate.UserEntity;
using Infrastructure.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UserServices.Domain;

internal sealed class IdentityUniquenessChecker(DomainDbContext context)
    : IIdentityUniquenessChecker
{
    public async Task CheckAsync(ExternalId identityId, CancellationToken cancellationToken)
    {
        // NOTE: This key might change in the future if we decide to support multiple identity providers.
        const string key = "github_id";

        bool isExisting = await context
            .Users.FromSql($"Select 1 From domain.users WHERE {key} = {identityId.Value}")
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (isExisting)
            throw new IdentityDuplicateException(identityId);
    }
}
