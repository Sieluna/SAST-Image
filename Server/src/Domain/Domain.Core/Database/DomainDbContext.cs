using Domain.Event;
using Domain.User;
using Microsoft.EntityFrameworkCore;

namespace Domain.Database;

public sealed class DomainDbContext(DbContextOptions<DomainDbContext> options) : DbContext(options)
{
    public const string Scheme = "domain";
    public required DbSet<DomainEventUnit> Events { get; init; }
    public required DbSet<UsernameRegistry> Usernames { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Model.SetValueGenerationStrategy(
            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.None
        );

        builder.HasDefaultSchema(Scheme);

        var e = builder.Entity<DomainEventUnit>();
        e.HasKey(e => e.EventId);
        e.HasIndex(e => new { e.GrainId, e.ETag }).IsUnique();
        e.HasIndex(e => e.Timestamp).IsUnique();
        e.HasIndex(e => e.Type).IsUnique(false);
        e.Property(e => e.Value).HasColumnType("jsonb");

        var username = builder.Entity<UsernameRegistry>();
        username.HasKey(e => new { e.UserId, e.Username });
        username.HasIndex(e => e.UserId).IsUnique();
        username.HasIndex(e => e.Username).IsUnique();
        username.Property<uint>("Version").IsRowVersion();
        username.Property(u => u.UserId).HasConversion(u => u.Value, v => new(v));
        username.Property(u => u.Username).HasConversion(u => u.Value, v => new(v));
    }
}
