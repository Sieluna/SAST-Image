using Domain.Extensions;
using Domain.UserAggregate.UserEntity;

namespace Domain.UserAggregate.Exceptions;

public sealed class IdentityDuplicateException(ExternalId id) : DomainException
{
    public ExternalId Id { get; } = id;
}
