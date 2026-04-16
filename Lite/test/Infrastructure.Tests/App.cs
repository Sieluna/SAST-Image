using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests;

[TestClass]
public static class App
{
    public static IServiceProvider Services { get; private set; } = null!;

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        var collection = new ServiceCollection();

        var configuration = new ConfigurationBuilder();
        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonStream(
            new MemoryStream(
                Encoding.UTF8.GetBytes(
                    """
                    {
                        "ConnectionStrings": {
                            "Database": "Server=10.0.0.153;Database=sastimg;User Id=postgres;Password=123456;",
                        },
                        "Storage": {
                            "BasePath": "./storage"
                        }
                    }
                    """
                )
            )
        );

        collection.AddInfrastructureServices(configuration.Build());

        Services = collection.BuildServiceProvider();
    }
}
