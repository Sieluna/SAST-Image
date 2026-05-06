using Microsoft.EntityFrameworkCore;

namespace Domain;

public sealed class DomainDbContext(DbContextOptions<DomainDbContext> options) : DbContext(options)
{
    public const string Scheme = "domain";
    public required DbSet<DomainStateUnit> Snapshots { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Scheme);

        var snapshots = builder.Entity<DomainStateUnit>();
        snapshots.HasKey(s => s.Id);
        snapshots.HasIndex(s => s.ETag);
        snapshots.Property(s => s.Value).HasColumnType("jsonb");
    }
}
