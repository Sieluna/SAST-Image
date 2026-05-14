using Domain.ValueObject;
using Orleans.Concurrency;

namespace Storage;

[Alias("ImageFileManager")]
public interface IImageFileManagerGrain : IGrainWithGuidKey
{
    [Alias(nameof(GetAsync))]
    public Task<Immutable<byte[]?>> GetAsync<TId>(
        TId id,
        string extension,
        CancellationToken cancellationToken = default
    )
        where TId : ITypedId<TId>;
}
