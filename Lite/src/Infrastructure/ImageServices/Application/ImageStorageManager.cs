using Application.ImageServices;
using Domain.AlbumAggregate.ImageEntity;
using Domain.Shared;
using Infrastructure.Shared.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.ImageServices.Application;

internal sealed class ImageStorageManager(
    ICompressProcessor processor,
    IOptions<StorageOptions> options
) : StorageManagerBase(options.Value.BasePath), IImageStorageManager
{
    private readonly string CompressedFilename = "compressed.webp";

    public Task DeleteImageAsync(ImageId image, CancellationToken cancellationToken = default)
    {
        Delete(image);
        return Task.CompletedTask;
    }

    public Stream? OpenReadStream(ImageId image, ImageKind kind)
    {
        string mask = kind switch
        {
            ImageKind.Original => "original.*",
            ImageKind.Thumbnail => "compressed.*",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        return OpenRead(image, mask);
    }

    public async Task StoreImageAsync(
        ImageId imageId,
        IImageFile imageFile,
        CancellationToken cancellationToken = default
    )
    {
        string originalFilename = $"original.{imageFile.Extesion}";

        await using var original = OpenWrite(imageId, originalFilename);
        imageFile.Stream.CopyTo(original);
        await using var compressed = OpenWrite(imageId, CompressedFilename);
        processor.CompressTo(imageFile.Stream, compressed);
    }
}

file static class ImageFileExtension
{
    extension<T>(T file)
        where T : IImageFile
    {
        public string Extesion
        {
            get
            {
                string loader = NetVips.Image.FindLoadStream(file.Stream);

                var span = loader.AsSpan();
                Span<char> buffer = stackalloc char[span.Length];
                span.ToLowerInvariant(buffer);

                const string prefix = "vipsforeignload";
                const string suffix = "file";

                var trimmed = buffer;
                if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    trimmed = trimmed[prefix.Length..];

                if (trimmed.EndsWith(suffix, StringComparison.Ordinal))
                    trimmed = trimmed[..^suffix.Length];

                return new string(trimmed);
            }
        }
    }
}
