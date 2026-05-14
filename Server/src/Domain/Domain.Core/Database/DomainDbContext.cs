using Domain.Event;
using Microsoft.EntityFrameworkCore;

namespace Domain.Database;

public sealed class DomainDbContext(DbContextOptions<DomainDbContext> options) : DbContext(options)
{
    public const string Scheme = "domain";
    public required DbSet<DomainEventUnit> Events { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Model.SetValueGenerationStrategy(
            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.None
        );

        builder.HasDefaultSchema(Scheme);

        var events = builder.Entity<DomainEventUnit>();
        events.HasKey(e => e.EventId);
        events.HasIndex(e => new { e.GrainId, e.ETag }).IsUnique();
        events.HasIndex(e => e.Timestamp);
        events.Property(e => e.Value).HasColumnType("jsonb");
    }
}
