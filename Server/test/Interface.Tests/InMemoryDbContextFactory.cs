using Microsoft.EntityFrameworkCore;
using Query.Database;

namespace Interface.Tests;

internal sealed class InMemoryDbContextFactory : IDbContextFactory<QueryDbContext>
{
    private readonly DbContextOptions<QueryDbContext> _options;

    public InMemoryDbContextFactory(string dbName)
    {
        _options = new DbContextOptionsBuilder<QueryDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;
    }

    public QueryDbContext CreateDbContext()
    {
        return (QueryDbContext)Activator.CreateInstance(typeof(QueryDbContext), _options)!;
    }
}
