using Application.UserServices;
using Domain.Shared;
using Domain.UserAggregate.UserEntity;
using Infrastructure.Shared.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.UserServices.Application;

internal sealed class HeaderStorageManager(
    ICompressProcessor processor,
    IOptions<StorageOptions> options
) : StorageManagerBase(options.Value.BasePath), IHeaderStorageManager
{
    private const string filename = "header.webp";

    public Stream? OpenReadStream(UserId user)
    {
        return OpenRead(user, filename);
    }

    public async Task UpdateAsync(
        UserId user,
        IImageFile header,
        CancellationToken cancellationToken
    )
    {
        await using var target = OpenWrite(user, filename);
        processor.CompressTo(header.Stream, target);
    }
}
