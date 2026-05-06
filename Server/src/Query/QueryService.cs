using Domain.Event;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Query.Database;

namespace Query;

public sealed partial class QueryService(IGrainFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await factory.GetGrain<IQueryServiceInnerGrain>(0).StartAsync(stoppingToken);
    }
}

[Alias("query_service")]
internal interface IQueryServiceInnerGrain : IGrainWithIntegerKey
{
    [Alias("query_service_start")]
    ValueTask StartAsync(CancellationToken cancellationToken);
}

public sealed partial class QueryServiceInnerGrain(ILogger<QueryService> logger)
    : Grain,
        IQueryServiceInnerGrain
{
    private bool started = false;
    private IGrainTimer timer = null!;

    private async Task Sync(CancellationToken cancellationToken)
    {
        //await using var eventContext = await eventFactory.CreateDbContextAsync(cancellationToken);
        //await using var queryContext = await queryFactory.CreateDbContextAsync(cancellationToken);

        await using var scope = ServiceProvider.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<QueryDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var checkpoint = await GetCheckPointAsync(context, cancellationToken);

        var events = await scope
            .ServiceProvider.GetRequiredService<EventDbContext>()
            .Events.AsNoTracking()
            .OrderBy(e => e.Timestamp)
            .Where(e => e.Timestamp > checkpoint.LastProcessedTimestamp)
            .ToArrayAsync(cancellationToken);

        if (events.Length <= 0)
            return;

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            await GetCheckPointAsync(context, cancellationToken);

            foreach (var e in events)
            {
                await mediator.Publish(e.Value, cancellationToken);
            }

            checkpoint.LastProcessedTimestamp = events[^1].Timestamp;
            checkpoint.LastUpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            LogFailedMessage(logger, ex, Array.ConvertAll(events, e => e.EventId));
        }
    }

    [LoggerMessage(LogLevel.Warning, "Failed to process outbox events {Events}")]
    static partial void LogFailedMessage(
        ILogger<QueryService> logger,
        Exception exception,
        Guid[] events
    );

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        if (started)
            return;

        await using var context = await ServiceProvider
            .GetRequiredService<IDbContextFactory<QueryDbContext>>()
            .CreateDbContextAsync(cancellationToken);

        await GetCheckPointAsync(context, cancellationToken);

        timer = this.RegisterGrainTimer(
            Sync,
            new GrainTimerCreationOptions()
            {
                DueTime = TimeSpan.Zero,
                Period = TimeSpan.FromSeconds(1),
                KeepAlive = true,
            }
        );
        started = true;
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
}
