using Domain.User;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Silo.Services;

public class UsernameUniquenessChecker(IDbContextFactory<QueryDbContext> factory)
    : IUsernameUniquenessChecker
{
    public async ValueTask<bool> ExistsAsync(
        Username username,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        return await context.Users.AnyAsync(
            i => EF.Functions.ILike(i.Username, username.Value),
            cancellationToken
        );
    }
}
