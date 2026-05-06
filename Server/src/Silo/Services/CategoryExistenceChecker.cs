using Domain.Album;
using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Silo.Services;

public sealed class CategoryExistenceChecker(IDbContextFactory<QueryDbContext> factory)
    : ICategoryExistenceChecker
{
    public async ValueTask<bool> ExistsAsync(
        long categoryId,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
    }
}
