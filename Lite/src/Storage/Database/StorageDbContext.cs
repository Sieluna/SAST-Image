using Domain.AlbumAggregate.AlbumEntity;
using Domain.AlbumAggregate.ImageEntity;
using Domain.Entity;
using Domain.Shared;
using Domain.UserAggregate.UserEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Storage.Images.Messages;
using Storage.Users.Messages;

namespace Storage.Database;

internal sealed class StorageDbContext(DbContextOptions<StorageDbContext> options)
    : DbContext(options)
{
    public required DbSet<OutboxMessage> Messages { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("storage");

        builder
            .Entity<OutboxMessage>()
            .HasDiscriminator<string>("type")
            .HasMessage<ImageAddedMessage>()
            .HasMessage<ImageDeletedMessage>()
            .HasMessage<AvatarUpdatedMessage>()
            .HasMessage<HeaderUpdatedMessage>()
            .IsComplete();

        builder.Entity<OutboxMessage>().HasKey(m => m.Id);

        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<ImageFile>().HaveConversion<ImageFileConverter>();
        builder.Id<ImageId>();
        builder.Id<AlbumId>();
        builder.Id<UserId>();

        base.ConfigureConventions(builder);
    }
}

file static class Extensions
{
    extension(DiscriminatorBuilder<string> builder)
    {
        public DiscriminatorBuilder<string> HasMessage<TMessage>()
            where TMessage : OutboxMessage, IOutboxMessage
        {
            return builder.HasValue<TMessage>(TMessage.Type);
        }
    }

    extension(ModelConfigurationBuilder builder)
    {
        public void Id<TId>()
            where TId : ITypedId<TId, long>, new()
        {
            builder.Properties<TId>().HaveConversion<TypedIdConverter<TId, long>>();
        }
    }
}

file sealed class TypedIdConverter<TId, TValue>()
    : ValueConverter<TId, TValue>(id => id.Value, value => new TId() { Value = value })
    where TId : ITypedId<TId, TValue>, new()
    where TValue : IEquatable<TValue>;

file sealed class ImageFileConverter()
    : ValueConverter<ImageFile, string>(image => image.Value, value => new() { Value = value }) { }
