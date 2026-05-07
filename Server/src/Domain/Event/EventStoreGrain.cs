using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;

namespace Domain.Event;

[StatelessWorker]
internal sealed class EventStoreGrain(IDbContextFactory<DomainDbContext> factory)
    : Grain,
        IEventStoreGrain
{
    public async IAsyncEnumerable<DomainEventUnit> GetEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var events = context.Events.OrderBy(e => e.Timestamp).AsAsyncEnumerable();
        await foreach (var @event in events.WithCancellation(cancellationToken))
            yield return @event;
    }

    public async IAsyncEnumerable<DomainEventUnit> GetEventsAsync(
        DateTime from,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var events = context
            .Events.Where(e => e.Timestamp >= from)
            .OrderBy(e => e.Timestamp)
            .AsAsyncEnumerable();
        await foreach (var @event in events.WithCancellation(cancellationToken))
            yield return @event;
    }

    public async IAsyncEnumerable<DomainEventUnit> GetEventsAsync(
        DateTime from,
        DateTime to,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var events = context
            .Events.Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .OrderBy(e => e.Timestamp)
            .AsAsyncEnumerable();
        await foreach (var @event in events.WithCancellation(cancellationToken))
            yield return @event;
    }
}
