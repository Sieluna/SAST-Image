using System.Diagnostics.CodeAnalysis;
using Domain.Entity;

namespace Domain.Shared;

/// <summary>
/// Represents a value object that encapsulates the path to an existing image file.
/// </summary>
/// <remarks>An ImageFile instance guarantees that the underlying file path refers to an existing file at the time
/// of creation. Use the TryCreateNew method to safely construct an ImageFile from a string path. This type is immutable
/// and can be used to enforce file existence constraints in domain models.</remarks>
public readonly record struct ImageFile
    : IValueObject<ImageFile, string>,
        IFactoryConstructor<ImageFile, string>
{
    public string Value
    {
        get =>
            File.Exists(field)
                ? field
                : throw new DomainModelInvalidException(
                    field,
                    $"{nameof(ImageFile)}.{nameof(Value)}"
                );
        init;
    }

    public static bool TryCreateNew(string input, [NotNullWhen(true)] out ImageFile newObject)
    {
        newObject = default;

        if (File.Exists(input) is false)
        {
            return false;
        }

        newObject = new ImageFile() { Value = input };
        return true;
    }
}
