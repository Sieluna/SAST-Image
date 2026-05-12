using Amazon.S3;
using Amazon.S3.Model;

namespace S3Storage;

public sealed class S3StorageService
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public S3StorageService(S3StorageOptions options)
    {
        _bucket = options.BucketName;
        var config = new AmazonS3Config
        {
            ServiceURL = options.ServiceUrl,
            ForcePathStyle = true,
            UseHttp = options.UseHttp,
        };
        _client = new AmazonS3Client(options.AccessKey, options.SecretKey, config);
    }

    public async Task EnsureBucketAsync(CancellationToken ct = default)
    {
        try
        {
            await _client.GetBucketLocationAsync(_bucket, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _client.PutBucketAsync(_bucket, ct);
        }
    }

    public async Task PutAsync(string key, Stream data, string contentType = "application/octet-stream", CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = data,
            ContentType = contentType,
        };
        await _client.PutObjectAsync(request, ct);
    }

    public async Task PutBytesAsync(string key, byte[] data, string contentType = "application/octet-stream", CancellationToken ct = default)
    {
        using var ms = new MemoryStream(data);
        await PutAsync(key, ms, contentType, ct);
    }

    public async Task<Stream?> GetAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetObjectAsync(_bucket, key, ct);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<byte[]?> GetBytesAsync(string key, CancellationToken ct = default)
    {
        await using var stream = await GetAsync(key, ct);
        if (stream is null) return null;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _client.GetObjectMetadataAsync(_bucket, key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await _client.DeleteObjectAsync(_bucket, key, ct);
    }

    public async Task<string[]> ListAsync(string? prefix = null, CancellationToken ct = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucket,
            Prefix = prefix,
        };
        var response = await _client.ListObjectsV2Async(request, ct);
        return response.S3Objects.Select(o => o.Key).ToArray();
    }
}

public sealed record S3StorageOptions
{
    public string ServiceUrl { get; init; } = "http://127.0.0.1:9000";
    public string AccessKey { get; init; } = "rustfs";
    public string SecretKey { get; init; } = "rustfs123";
    public string BucketName { get; init; } = "sastimg";
    public bool UseHttp { get; init; } = true;
}
