using Microsoft.EntityFrameworkCore;

namespace Domain.Event;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public const string Scheme = "event";

    public required DbSet<DomainEventUnit> Events { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Scheme);

        var events = builder.Entity<DomainEventUnit>();
        events.HasKey(e => e.EventId);
        events.HasIndex(e => new { e.GrainId, e.ETag }).IsUnique();
        events.HasIndex(e => e.Timestamp);
        events.Property(e => e.Value).HasColumnType("jsonb");
    }
}
