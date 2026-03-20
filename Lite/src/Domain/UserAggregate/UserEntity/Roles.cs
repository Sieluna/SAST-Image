using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Domain.Entity;

namespace Domain.UserAggregate.UserEntity;

[CollectionBuilder(typeof(RolesCollectionBuilder), nameof(RolesCollectionBuilder.Build))]
public readonly struct Roles
    : IEnumerable<Role>,
        IValueObject<Roles, Role[]>,
        IFactoryConstructor<Roles, Role[]>
{
    public Role[] Value { get; }

    internal Roles(Role[] value) => Value = value;

    public static bool TryCreateNew(
        Role[] input,
        [MaybeNullWhen(false), NotNullWhen(true)] out Roles newObject
    )
    {
        if (input.Any(role => !Enum.IsDefined(role)))
        {
            newObject = default;
            return false;
        }

        var mid = input.Distinct().ToArray();

        newObject = new(mid);
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Roles roles && Equals(roles);
    }

    public bool Equals(Roles other)
    {
        return Value.SequenceEqual(other.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public IEnumerator<Role> GetEnumerator() => (Value as IEnumerable<Role>).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

file sealed class RolesCollectionBuilder
{
    public static Roles Build(ReadOnlySpan<Role> span)
    {
        return new Roles(span.ToArray());
    }
}
