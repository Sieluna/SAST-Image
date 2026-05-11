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
    IServiceScopeFactory scoper,
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
        await using var scope = scoper.CreateAsyncScope();
        var services = scope.ServiceProvider;
        await using var context = services.GetRequiredService<QueryDbContext>();
        var mediator = services.GetRequiredService<IMediator>();

        var (main, points) = await GetCheckpointsAsync(context, cancellationToken);
        foreach (var e in await store.GetEventsAsync(main.Timestamp, cancellationToken))
        {
            await using var transaction = await context.Database.BeginTransactionAsync(
                cancellationToken
            );

            if (points.Any(p => p.GrainId == e.GrainId && p.Status == CheckpointStatus.Failed))
                continue;

            try
            {
                main.Timestamp = e.Timestamp;
                await mediator.Publish(e.Value, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Silence and skip.
                return;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                context.ChangeTracker.Clear();
                Checkpoint failed = new()
                {
                    Id = e.EventId,
                    Timestamp = e.Timestamp,
                    GrainId = e.GrainId,
                    Status = CheckpointStatus.Failed,
                };
                points.Add(failed);
                await context.Checkpoints.AddAsync(failed, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                LogFailedMessage(logger, ex, e.EventId);
            }
        }
    }

    private static async Task<(Checkpoint main, List<Checkpoint> all)> GetCheckpointsAsync(
        QueryDbContext context,
        CancellationToken cancellationToken
    )
    {
        var points = await context.Checkpoints.ToListAsync(cancellationToken);
        var main = points.FirstOrDefault(cp => cp.GrainId is null);
        if (points.Count <= 0 || main is null)
        {
            main = new Checkpoint { Timestamp = DateTime.MinValue };
            points = [main];
            await context.Checkpoints.AddAsync(main, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return (main, points);
        }

        return (main, points);
    }

    [LoggerMessage(LogLevel.Warning, "Failed to process outbox event {Event}")]
    static partial void LogFailedMessage(
        ILogger<QueryService> logger,
        Exception exception,
        Guid @event
    );
}
