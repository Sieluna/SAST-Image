using Domain.Event;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Query.Database;

namespace Query;

public sealed partial class QueryService(
    IGrainFactory factory,
    IServiceProvider provider,
    ILogger<QueryService> logger
) : BackgroundService
{
    const int intervalSeconds = 1;

    private readonly IEventStoreGrain store = factory.GetGrain<IEventStoreGrain>(Guid.Empty);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await Sync(stoppingToken);
    }

    private async Task Sync(CancellationToken cancellationToken)
    {
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<QueryDbContext>();
        var mediator = services.GetRequiredService<IMediator>();

        var checkpoint = await GetCheckPointAsync(context, cancellationToken);
        await foreach (
            var e in store.GetEventsAsync(checkpoint.LastProcessedTimestamp, cancellationToken)
        )
        {
            try
            {
                await mediator.Publish(e.Value, cancellationToken);
                checkpoint.LastProcessedTimestamp = e.Timestamp;
                checkpoint.LastUpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                LogFailedMessage(logger, ex, e.EventId);
            }
        }
    }

    private static async ValueTask<Checkpoint> GetCheckPointAsync(
        QueryDbContext context,
        CancellationToken cancellationToken
    )
    {
        var cp = await context.Checkpoint.SingleOrDefaultAsync(cancellationToken);
        var checkpoint =
            cp
            ?? new()
            {
                LastProcessedTimestamp = DateTime.MinValue,
                LastUpdatedAt = DateTime.UtcNow,
            };

        if (cp is null)
        {
            await context.Checkpoint.AddAsync(checkpoint, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        return checkpoint;
    }

    [LoggerMessage(LogLevel.Warning, "Failed to process outbox event {Event}")]
    static partial void LogFailedMessage(
        ILogger<QueryService> logger,
        Exception exception,
        Guid @event
    );
}
