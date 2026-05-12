using Microsoft.Extensions.DependencyInjection;
using Client.Storage;

namespace Client;

public static class ClientExtensions
{
    /// <summary>
    /// Registers <see cref="Client"/> as a singleton in the DI container.
    /// </summary>
    /// <param name="baseUrl">Base URL of the SAST-Image API.</param>
    public static IServiceCollection AddClient(
        this IServiceCollection services,
        string baseUrl = "http://localhost:5265")
    {
        services.AddSingleton(_ => new Client(baseUrl));
        return services;
    }

    /// <summary>
    /// Registers <see cref="Client"/> as a singleton with custom storage.
    /// </summary>
    public static IServiceCollection AddClient(
        this IServiceCollection services,
        IStorage storage,
        string baseUrl = "http://localhost:5265")
    {
        services.AddSingleton(_ => new Client(
            new ClientOptions { BaseUrl = baseUrl, Storage = storage }));
        return services;
    }

    /// <summary>
    /// Registers <see cref="Client"/> as a singleton with full options.
    /// </summary>
    public static IServiceCollection AddClient(
        this IServiceCollection services,
        ClientOptions options)
    {
        services.AddSingleton(_ => new Client(options));
        return services;
    }

    /// <summary>
    /// Registers <see cref="Client"/> as a singleton using an existing <see cref="HttpClient"/>
    /// (from <c>IHttpClientFactory</c> or elsewhere).
    /// </summary>
    public static IServiceCollection AddClient(
        this IServiceCollection services,
        HttpClient httpClient,
        IStorage? storage = null)
    {
        storage ??= new FileStorage();
        services.AddSingleton(_ => new Client(
            new ClientOptions { HttpClient = httpClient, Storage = storage }));
        return services;
    }
}
