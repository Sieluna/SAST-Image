using System.Diagnostics.CodeAnalysis;
using Domain.ValueObject;

namespace Domain.Album.Image;

[OpenJsonConverter<ImageDescription, string>]
public readonly record struct ImageDescription(string Value)
    : IValueObject<ImageDescription, string>
{
    public const int MaxLength = 32;

    public static readonly ImageDescription Empty = new(string.Empty);

    public static bool TryCreateNew(
        string value,
        [NotNullWhen(true)] out ImageDescription newObject
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            newObject = Empty;
            return true;
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            newObject = default;
            return false;
        }

        newObject = new(value);
        return true;
    }
}
