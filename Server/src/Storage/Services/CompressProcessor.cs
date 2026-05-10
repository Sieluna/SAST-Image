using Domain.ValueObject;
using Microsoft.Extensions.Options;
using NetVips;

namespace Storage.Services;

public interface ICompressProcessor
{
    public ValueTask CompressAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : ITypedId<TId>;

    public ValueTask CompressAsync<TId>(
        TId id,
        string? sourceSuffix,
        CancellationToken cancellationToken = default
    )
        where TId : ITypedId<TId>;

    public ValueTask CompressAsync<TId>(
        TId id,
        string? sourceSuffix,
        string? destinationSuffix,
        CancellationToken cancellationToken = default
    )
        where TId : ITypedId<TId>;
}

internal sealed class LocalCompressProcessor(IOptions<StorageOptions> options) : ICompressProcessor
{
    public async ValueTask CompressAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : ITypedId<TId>
    {
        await CompressAsync(id, null, cancellationToken);
    }

    public async ValueTask CompressAsync<TId>(
        TId id,
        string? sourceExtension,
        CancellationToken cancellationToken = default
    )
        where TId : ITypedId<TId>
    {
        await CompressAsync(id, sourceExtension, sourceExtension, cancellationToken);
    }

    public async ValueTask CompressAsync<TId>(
        TId id,
        string? sourceExtension,
        string? destinationExtension,
        CancellationToken cancellationToken = default
    )
        where TId : ITypedId<TId>
    {
        string original = id.AbsolutePath(options.Value.BaseUri.LocalPath, sourceExtension);
        string temp =
            Path.GetDirectoryName(original)
            + Path.DirectorySeparatorChar
            + Path.GetRandomFileName();

        using var image = Image.NewFromFile(original);

        image.Heifsave(
            temp,
            q: options.Value.Quality,
            effort: options.Value.Effort,
            compression: Enums.ForeignHeifCompression.Av1,
            keep: Enums.ForeignKeep.All,
            lossless: false
        );

        string compressed = Path.ChangeExtension(original, destinationExtension);
        File.Move(temp, compressed, true);
    }
}
