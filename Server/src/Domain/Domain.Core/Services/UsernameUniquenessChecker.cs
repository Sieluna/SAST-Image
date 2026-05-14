using Domain.Database;
using Domain.User;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

internal sealed class UsernameUniquenessChecker(IDbContextFactory<DomainDbContext> factory)
    : IUsernameUniquenessChecker
{
    public async ValueTask<bool> ExistsAsync(
        Username username,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
