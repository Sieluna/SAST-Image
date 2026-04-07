using Domain.Entity;

namespace Domain.UserAggregate.UserEntity;

/// <summary>
/// Represents a strongly-typed external (OAuth) identifier value object.
/// </summary>
/// <remarks>
/// Use this type to encapsulate external identifiers and provide type safety when working with IDs from external systems.
/// <br/>
/// This struct is immutable and supports value-based equality.
/// <br/>
/// Currently, this struct is defined with a <seealso cref="long"/> value for <strong>GitHub</strong>.
/// </remarks>
public readonly record struct ExternalId(long Value) : IValueObject<ExternalId, long>;
