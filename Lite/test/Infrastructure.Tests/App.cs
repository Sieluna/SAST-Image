namespace Infrastructure.Tests;

[TestClass]
public static class App
{
    public static IServiceProvider Services { get; private set; } = null!;

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context) { }
}
