using Domain.ValueObject;
using Orleans.Concurrency;
using Storage.Services;

namespace Storage;

[StatelessWorker]
internal sealed class ImageFileManagerGrain(LocalImageFileManager manager)
    : Grain,
        IImageFileManagerGrain
{
    public async Task<Immutable<byte[]?>> GetAsync<TId>(
        TId id,
        string? extension,
        CancellationToken cancellationToken = default
    )
        where TId : ITypedId<TId>
    {
        await using var stream = manager.GetStream(id, extension);

        if (stream is null)
            return new Immutable<byte[]?>(null);

        await using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream, LocalImageFileManager.BufferSize, cancellationToken);

        return new Immutable<byte[]?>(memoryStream.ToArray());
    }
}
