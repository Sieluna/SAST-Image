using Domain;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace Silo.Services;

public sealed class IdUniquenessChecker(IDbContextFactory<DomainDbContext> factory)
    : IIdUniquenessChecker
{
    public async ValueTask<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context.Snapshots.AnyAsync(s => s.Id == id, cancellationToken);
    }
}
