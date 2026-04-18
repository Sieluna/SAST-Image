using System.Diagnostics.CodeAnalysis;
using Domain.Entity;
using Domain.Shared.Converter;
using Domain.UserAggregate.UserEntity;

namespace Domain.AlbumAggregate.AlbumEntity;

[OpenJsonConverter<Collaborators, UserId[]>]
public sealed class Collaborators()
    : ValueObjects<Collaborators, UserId>,
        IFactoryConstructor<Collaborators, UserId[]>
{
    internal Collaborators(params UserId[] array)
        : this() => Value = array;

    public const int MaxCount = 32;

    public static bool TryCreateNew(
        UserId[] value,
        [MaybeNullWhen(false), NotNullWhen(true)] out Collaborators newObject
    )
    {
        newObject = default;

        if (value.Length == 0)
        {
            newObject = new();
            return true;
        }

        var set = new HashSet<UserId>(value);

        if (set.Count > MaxCount)
            return false;

        newObject = new([.. set]);

        return true;
    }
}
