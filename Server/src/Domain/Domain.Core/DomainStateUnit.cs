using System.Text.Json.Serialization;
using Domain.Album;
using Domain.Category;
using Domain.Event;
using Domain.User;

namespace Domain;

[JsonPolymorphic(
    TypeDiscriminatorPropertyName = "$type",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
)]
[DerivedState<CategoryState>]
[DerivedState<AlbumState>]
[DerivedState<UserState>]
public abstract class DomainStateBase
{
    public bool RecordExists { get; set; } = false;
    public abstract void Apply(DomainEventBase e);
}

internal sealed class DerivedStateAttribute<TState> : DerivedAttribute<TState, DomainStateBase>
    where TState : DomainStateBase;

internal interface IDomainEventApplicable<TEvent>
    where TEvent : DomainEventBase
{
    void Apply(TEvent e);
}
