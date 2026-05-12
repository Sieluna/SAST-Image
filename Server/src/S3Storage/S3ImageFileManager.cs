using Domain.ValueObject;
using Storage.Services;

namespace S3Storage;

internal sealed class S3ImageFileManager(S3StorageService storage) : IImageFileManager
{
    public async ValueTask SaveAsync<TId>(
        Stream stream,
        TId id,
        CancellationToken cancellationToken = default)
        where TId : ITypedId<TId, long>
    {
        await SaveAsync(stream, id, null, cancellationToken);
    }

    public async ValueTask SaveAsync<TId>(
        Stream stream,
        TId id,
        string? extension,
        CancellationToken cancellationToken = default)
        where TId : ITypedId<TId, long>
    {
        var key = id.ToS3Key(extension);
        await storage.PutAsync(key, stream, ct: cancellationToken);
    }

    public Stream? GetStream<TId>(TId id)
        where TId : ITypedId<TId, long>
    {
        return GetStream(id, null);
    }

    public Stream? GetStream<TId>(TId id, string? extension)
        where TId : ITypedId<TId, long>
    {
        var key = id.ToS3Key(extension);
        return storage.GetAsync(key).GetAwaiter().GetResult();
    }

    public async ValueTask DeleteAsync<TId>(
        TId id,
        CancellationToken cancellationToken = default)
        where TId : ITypedId<TId, long>
    {
        var key = id.ToS3Key();
        await storage.DeleteAsync(key, cancellationToken);
    }
}

internal static class S3KeyExtensions
{
    internal static string ToS3Key<TId>(this TId id, string? extension = null)
        where TId : ITypedId<TId, long>
    {
        // Use same path pattern as LocalImageFileManager for consistency
        var relativePath = id.RelativePath();
        return extension is not null
            ? $"{relativePath}.{extension}"
            : relativePath;
    }

    private static string RelativePath<TId>(this TId id)
        where TId : ITypedId<TId, long>
    {
        const long epoch = 1288834974657L; // Twitter Snowflake epoch
        long timestamp = id.Value >> 22;

        if (timestamp <= 0)
            return id.Value.ToString();

        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(epoch + timestamp);
        return dateTime.ToString($"yyyy/MM/dd/{id.Value}");
    }
}
