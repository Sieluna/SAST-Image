using Application.AlbumServices;
using Application.ImageServices;
using Domain.AlbumAggregate.AlbumEntity;
using Domain.AlbumAggregate.ImageEntity;
using Domain.Shared;
using Infrastructure.Shared.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.AlbumServices.Application;

internal sealed class CoverStorageManager(
    ICompressProcessor compressor,
    IImageStorageManager images,
    IOptions<StorageOptions> options
) : StorageManagerBase(options.Value.BasePath), ICoverStorageManager
{
    private const string filename = "cover.webp";

    public Stream? OpenReadStream(AlbumId album)
    {
        return OpenRead(album, filename);
    }

    public Task DeleteCoverAsync(AlbumId album, CancellationToken cancellationToken = default)
    {
        Delete(album);
        return Task.CompletedTask;
    }

    public async Task UpdateWithContainedImageAsync(
        AlbumId album,
        ImageId image,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = images.OpenReadStream(image, ImageKind.Thumbnail);

        if (stream == null)
            return;

        await using var target = OpenWrite(album, filename);
        compressor.CompressTo(stream, target);
    }

    public async Task UpdateWithCustomImageAsync(
        AlbumId album,
        IImageFile file,
        CancellationToken cancellationToken = default
    )
    {
        await using var target = OpenWrite(album, filename);

        compressor.CompressTo(file.Stream, target);
    }
}
