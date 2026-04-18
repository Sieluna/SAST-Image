using Domain.AlbumAggregate.AlbumEntity;
using Domain.AlbumAggregate.ImageEntity;
using Domain.CategoryAggregate.CategoryEntity;
using Domain.Entity;
using Domain.UserAggregate.IdentityEntity;
using Domain.UserAggregate.UserEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Shared.Database.ModelBuild;

internal static class ModelConfigurationBuilderExtensions
{
    extension(ModelConfigurationBuilder builder)
    {
        public void UseDefaultMapping()
        {
            builder.MapValueObject<AccessLevel, AccessLevelValue>();
            builder.MapValueObject<RefreshToken, string>();
            builder.MapValueObject<CategoryName, string>();
            builder.MapValueObject<IdentityId, long>();
            builder.MapValueObject<Username, string>();
            builder.MapValueObject<Email, string>();

            builder.MapValueObjects<Roles, Role>();
            builder.MapValueObjects<Collaborators, UserId>();
            //builder.MapValueObjects<ImageTags, string>();
            //builder.MapValueObjects<AlbumTags, string>();

            builder.MapTypedId<UserId>();
            builder.MapTypedId<ImageId>();
            builder.MapTypedId<AlbumId>();
            builder.MapTypedId<CategoryId>();
        }

        private TypeMappingConfigurationBuilder<TObject> MapValueObject<TObject, TValue>()
            where TObject : IValueObject<TObject, TValue>, new()
        {
            builder.Properties<TObject>().HaveConversion<ValueObjectConverter<TObject, TValue>>();
            return builder
                .DefaultTypeMapping<TObject>()
                .HasConversion<ValueObjectConverter<TObject, TValue>>();
        }

        private TypeMappingConfigurationBuilder<TObject> MapValueObjects<TObject, TValue>()
            where TObject : ValueObjects<TObject, TValue>, new()
        {
            builder.Properties<TObject>().HaveConversion<ValueObjectsConverter<TObject, TValue>>();
            return builder
                .DefaultTypeMapping<TObject>()
                .HasConversion<ValueObjectsConverter<TObject, TValue>>();
        }

        private TypeMappingConfigurationBuilder<TId> MapTypedId<TId, TValue>()
            where TId : ITypedId<TId, TValue>, new()
            where TValue : IEquatable<TValue>
        {
            builder.Properties<TId>().HaveConversion<TypedIdConverter<TId, TValue>>();
            return builder.DefaultTypeMapping<TId>().HasConversion<TypedIdConverter<TId, TValue>>();
        }

        private TypeMappingConfigurationBuilder<TId> MapTypedId<TId>()
            where TId : ITypedId<TId, long>, new()
        {
            return builder.MapTypedId<TId, long>();
        }
    }
}

file sealed class ValueObjectConverter<TObject, TValue>()
    : ValueConverter<TObject, TValue>(id => id.Value, value => new() { Value = value })
    where TObject : IValueObject<TObject, TValue>, new() { }

file sealed class ValueObjectsConverter<TObject, TValue>()
    : ValueConverter<TObject, TValue[]>(obj => obj.Value, value => new() { Value = value })
    where TObject : ValueObjects<TObject, TValue>, new() { }

file sealed class ValueObjectsComparer<TObject, TValue>()
    : ValueComparer<TObject>(
        (c1, c2) => EqualityComparer<TObject>.Default.Equals(c1, c2),
        c => c.GetHashCode()
    )
    where TObject : ValueObjects<TObject, TValue>, new() { }

file sealed class TypedIdConverter<TId, TValue>()
    : ValueConverter<TId, TValue>(id => id.Value, value => new() { Value = value })
    where TId : ITypedId<TId, TValue>, new()
    where TValue : IEquatable<TValue> { }
