using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Domain.Entity;
using Domain.Shared.Converter;
using Domain.UserAggregate.UserEntity;

namespace Domain.AlbumAggregate.AlbumEntity;

[CollectionBuilder(
    typeof(CollaboratorsCollectionBuilder),
    nameof(CollaboratorsCollectionBuilder.Build)
)]
[OpenJsonConverter<Collaborators, UserId[]>]
public readonly struct Collaborators
    : IValueObject<Collaborators, UserId[]>,
        IEnumerable<UserId>,
        IFactoryConstructor<Collaborators, UserId[]>
{
    public const int MaxCount = 32;

    public UserId[] Value { get; }

    internal Collaborators(UserId[] array) => Value = array;

    public static bool TryCreateNew(
        UserId[] value,
        [MaybeNullWhen(false), NotNullWhen(true)] out Collaborators newObject
    )
    {
        var mid = value.Distinct().ToArray();

        if (mid.Length > MaxCount)
        {
            newObject = default;
            return false;
        }

        newObject = new(mid);
        return true;
    }

    public bool Equals(Collaborators other)
    {
        return Value.SequenceEqual(other.Value); // TODO: optimize sorting and comparing.
    }

    public override bool Equals(object? obj)
    {
        return obj is Collaborators collaborators && Equals(collaborators);
    }

    public static bool operator ==(Collaborators left, Collaborators right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Collaborators left, Collaborators right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public IEnumerator<UserId> GetEnumerator() => ((IEnumerable<UserId>)Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Value.GetEnumerator();

    public static readonly Collaborators Empty = new([]);
}

file sealed class CollaboratorsCollectionBuilder
{
    public static Collaborators Build(ReadOnlySpan<UserId> value)
    {
        if (!Collaborators.TryCreateNew(value.ToArray(), out var collaborators))
        {
            throw new ArgumentException(
                $"The number of collaborators cannot exceed {Collaborators.MaxCount}."
            );
        }
        return collaborators;
    }
}
