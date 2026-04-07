using Domain.UserAggregate.Exceptions;
using Domain.UserAggregate.Services;
using Domain.UserAggregate.UserEntity;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.UserServices.Domain;

internal sealed class RegistryCodeChecker(IDistributedCache cache) : IRegistryCodeChecker
{
    public async Task CheckAsync(
        Email email,
        RegistryCode code,
        CancellationToken cancellationToken
    )
    {
        string? value = await cache.GetStringAsync(code.Value.ToString(), cancellationToken);

        if (long.TryParse(value, out long stored) is false || stored != code.Value)
        {
            throw new RegistryCodeException();
        }
    }
}
