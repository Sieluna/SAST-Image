using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Orleans.Concurrency;

namespace Domain.File;

[StatelessWorker]
internal sealed class FileManagerGrain(IDbContextFactory<DomainDbContext> factory)
    : Grain,
        IFileManagerGrain
{
    const int BufferSize = 1024 * 64;

    public async IAsyncEnumerable<byte[]> GetFileAsync(
        ImageFileKey fileKey,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        await using var connection = (NpgsqlConnection)context.Database.GetDbConnection();

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        NpgsqlLargeObjectManager manager = new(connection);
        await using var stream = await manager.OpenReadAsync(fileKey.Value, cancellationToken);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        int bytesRead;
        try
        {
            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return buffer[..bytesRead];
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task<ImageFileKey> UploadFileAsync(
        IAsyncEnumerable<byte[]> fileStream,
        CancellationToken cancellationToken
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        await using var connection = context.Database.GetDbConnection() as NpgsqlConnection;

        await using var transaction = await connection!.BeginTransactionAsync(cancellationToken);
        NpgsqlLargeObjectManager manager = new(connection);
        var key = await manager.CreateAsync(0, cancellationToken);

        await using var stream = await manager.OpenReadWriteAsync(key, cancellationToken);

        await foreach (var chunk in fileStream.WithCancellation(cancellationToken))
        {
            await stream.WriteAsync(chunk, cancellationToken);
        }

        return new(key);
    }
}
