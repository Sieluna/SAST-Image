using Domain.Database;
using Domain.Event;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace Domain;

public abstract class DomainGrain<TState> : DomainGrain<TState, DomainEventBase>
    where TState : DomainStateBase, new();

public abstract class DomainGrain<TState, TEventBase>
    : JournaledGrain<TState, TEventBase>,
        ICustomStorageInterface<TState, TEventBase>,
        IDomainGrainStateRecordExistenceIndicator
    where TState : DomainStateBase, new()
    where TEventBase : DomainEventBase
{
    public bool RecordExists => State.RecordExists;

    protected long Id => this.GetPrimaryKeyLong();
    protected Actor Actor =>
        RequestContext.Get(nameof(Actor)) as Actor ?? throw new ForbiddenException();

    public async Task<bool> ApplyUpdatesToStorage(
        IReadOnlyList<TEventBase> updates,
        int expectedVersion
    )
    {
        if (updates.Count == 0)
            return true;

        var grainId = this.GetPrimaryKeyLong();

        await using var context = await ServiceProvider
            .GetRequiredService<IDbContextFactory<DomainDbContext>>()
            .CreateDbContextAsync();

        var currentVersion = await context
            .Events.AsNoTracking()
            .Where(e => e.GrainId == grainId)
            .Select(e => e.ETag)
            .OrderByDescending(e => e)
            .FirstOrDefaultAsync();

        if (currentVersion != expectedVersion)
            return false;

        List<DomainEventUnit> events = [];

        foreach (var e in updates)
        {
            currentVersion++;
            events.Add(
                new DomainEventUnit
                {
                    Value = e,
                    GrainId = grainId,
                    ETag = currentVersion,
                    Timestamp = e.Timestamp,
                }
            );
        }

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        var state = new TState();

        foreach (var e in updates)
            state.Apply(e);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<KeyValuePair<int, TState>> ReadStateFromStorage()
    {
        var grainId = this.GetPrimaryKeyLong();

        await using var context = await ServiceProvider
            .GetRequiredService<IDbContextFactory<DomainDbContext>>()
            .CreateDbContextAsync();

        var state = new TState();
        var events = await context
            .Events.AsNoTracking()
            .Where(e => e.GrainId == grainId)
            .OrderBy(e => e.ETag)
            .Select(e => e.Value)
            .ToArrayAsync();

        var version = 0;
        foreach (var e in events)
        {
            state.Apply((TEventBase)e);
            version++;
        }

        return new(version, state);
    }
}
