using Application.UserServices;
using Domain.Shared;
using Domain.UserAggregate.UserEntity;
using Infrastructure.Shared.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.UserServices.Application;

internal sealed class AvatarStorageManager(
    ICompressProcessor processor,
    IOptions<StorageOptions> options
) : StorageManagerBase(options.Value.BasePath), IAvatarStorageManager
{
    private const string filename = "avatar.webp";

    public Stream? OpenReadStream(UserId user)
    {
        return OpenRead(user, filename);
    }

    public async Task UpdateAsync(
        UserId user,
        IImageFile avatar,
        CancellationToken cancellationToken
    )
    {
        await using var target = OpenWrite(user, filename);

        processor.CompressTo(avatar.Stream, target);
    }
}
