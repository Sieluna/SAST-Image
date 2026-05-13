using Domain;
using Domain.Album.Image;
using Mediator;
using Microsoft.Extensions.Options;
using NetVips;
using Storage.Services;

namespace Storage.Image.Queries;

public sealed record ImageFileQuery(
    ImageId Image,
    ImageKind Kind,
    Actor Actor,
    string? Format = null
) : IQuery<Stream?>;

public sealed class ImageFileQueryHandler(
    IImageFileManager manager,
    IAccessChecker checker,
    IOptions<StorageOptions> storageOptions
) : IQueryHandler<ImageFileQuery, Stream?>
{
    public async ValueTask<Stream?> Handle(
        ImageFileQuery request,
        CancellationToken cancellationToken
    )
    {
        bool result = await checker.HasAccessAsync(request.Actor, request.Image, cancellationToken);
        if (result is false)
            return null;

        var format = request.Format ?? "avif";

        var stream = manager.GetStream(request.Image, format);
        if (stream is not null)
            return stream;

        // Convert from avif source on demand
        var avifStream = manager.GetStream(request.Image, "avif");
        if (avifStream is null)
            return null;

        using var image = Image.NewFromStream(avifStream);
        var q = storageOptions.Value.Quality;

        byte[] buffer = format switch
        {
            "webp" => image.WebpsaveBuffer(q: q, lossless: false),
            "jpeg" => image.JpegsaveBuffer(q: q),
            "png" => image.PngsaveBuffer(),
            _ => throw new NotSupportedException($"Unsupported format: {format}"),
        };

        var converted = new MemoryStream(buffer);
        await manager.SaveAsync(converted, request.Image, format, cancellationToken);
        converted.Position = 0;
        return converted;
    }
}
