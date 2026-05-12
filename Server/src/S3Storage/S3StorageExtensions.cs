using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace S3Storage;

public static class S3StorageExtensions
{
    public static IServiceCollection AddS3Storage(
        this IServiceCollection services,
        IConfiguration configuration,
        bool startRustFs = false)
    {
        var options = configuration.GetSection("S3Storage").Get<S3StorageOptions>()
            ?? new S3StorageOptions();

        services.AddSingleton(options);
        services.AddSingleton<S3StorageService>();

        if (startRustFs)
        {
            var rustFsOptions = new RustFsOptions
            {
                Port = new Uri(options.ServiceUrl).Port,
                DataDirectory = Path.Combine(AppContext.BaseDirectory, "rustfs-data"),
            };
            services.AddSingleton(rustFsOptions);
            services.AddSingleton<RustFsProcess>();
            services.AddHostedService<RustFsHostedService>();
        }

        return services;
    }
}

internal sealed class RustFsHostedService(
    RustFsProcess process,
    S3StorageService storage) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await process.StartAsync(cancellationToken);
        await storage.EnsureBucketAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        process.Dispose();
        return Task.CompletedTask;
    }
}
