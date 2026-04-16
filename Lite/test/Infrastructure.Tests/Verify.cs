using Infrastructure.Shared.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests;

[TestClass]
public sealed class Verify
{
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var domain = App.Services.GetRequiredService<DomainDbContext>().Database;
        await domain.EnsureDeletedAsync(context.CancellationToken);
        await domain.EnsureCreatedAsync(context.CancellationToken);

        await App
            .Services.GetRequiredService<QueryDbContext>()
            .Database.GetService<IRelationalDatabaseCreator>()
            .CreateTablesAsync(context.CancellationToken);
    }

    [TestMethod]
    public async Task VerifyDatabase() { }

    //[ClassCleanup]
    //public static async Task ClassCleanup(TestContext context)
    //{
    //    await App
    //        .Services.GetRequiredService<DomainDbContext>()
    //        .Database.EnsureDeletedAsync(context.CancellationToken);
    //}

    public TestContext TestContext { get; set; }
}
