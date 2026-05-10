namespace Domain.File;

[Alias("file_manager")]
public interface IFileManagerGrain : IGrainWithGuidKey
{
    [Alias("get_file")]
    public IAsyncEnumerable<byte[]> GetFileAsync(
        ImageFileKey fileKey,
        CancellationToken cancellationToken
    );

    [Alias("upload_file")]
    public Task<ImageFileKey> UploadFileAsync(
        IAsyncEnumerable<byte[]> fileStream,
        CancellationToken cancellationToken
    );
}
