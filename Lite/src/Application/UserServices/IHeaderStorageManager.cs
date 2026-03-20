using Domain.Shared;
using Domain.UserAggregate.UserEntity;

namespace Application.UserServices;

public interface IHeaderStorageManager
{
    public Task UpdateAsync(UserId user, IImageFile header, CancellationToken cancellationToken);
    public Stream? OpenReadStream(UserId user);
}
