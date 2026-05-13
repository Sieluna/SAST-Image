using Microsoft.Extensions.DependencyInjection;
using Client.Storage;

namespace Client.Http;

public static class ClientExtensions
{
    public static IServiceCollection AddClient(
        this IServiceCollection services,
        string baseUrl = "http://localhost:5265")
    {
        services.AddSingleton(_ => new HttpRestClient(baseUrl));
        return services;
    }

    public static IServiceCollection AddClient(
        this IServiceCollection services,
        IStorage storage,
        string baseUrl = "http://localhost:5265")
    {
        services.AddSingleton(_ => new HttpRestClient(
            new ClientOptions { BaseUrl = baseUrl, Storage = storage }));
        return services;
    }

    public static IServiceCollection AddClient(
        this IServiceCollection services,
        ClientOptions options)
    {
        services.AddSingleton(_ => new HttpRestClient(options));
        return services;
    }

    public static IServiceCollection AddClient(
        this IServiceCollection services,
        HttpClient httpClient,
        IStorage? storage = null)
    {
        storage ??= new FileStorage();
        services.AddSingleton(_ => new HttpRestClient(
            new ClientOptions { HttpClient = httpClient, Storage = storage }));
        return services;
    }
}
