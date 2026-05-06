using System.Text.Json.Serialization;
using Domain.Album.Events;
using Domain.Category.Events;
using Domain.User.Events;

namespace Domain.Event;

[JsonPolymorphic]
// Category
[DerivedEvent<CategoryCreatedEvent>]
[DerivedEvent<CategoryDeletedEvent>]
[DerivedEvent<CategoryUpdatedEvent>]
// User
[DerivedEvent<ProfileUpdatedEvent>]
[DerivedEvent<AvatarUpdatedEvent>]
[DerivedEvent<HeaderUpdatedEvent>]
[DerivedEvent<UserRegisteredEvent>]
//Album
[DerivedEvent<AlbumCreatedEvent>]
[DerivedEvent<AlbumRemovedEvent>]
[DerivedEvent<AlbumUpdatedEvent>]
[DerivedEvent<AlbumSubscribedEvent>]
[DerivedEvent<AlbumUnsubscribedEvent>]
[DerivedEvent<ImageAddedEvent>]
[DerivedEvent<ImageUpdatedEvent>]
[DerivedEvent<ImageRemovedEvent>]
public record class DomainEventBase(long Id) : Mediator.INotification
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public sealed class DerivedEventAttribute<TEvent> : DerivedAttribute<TEvent, DomainEventBase>
    where TEvent : DomainEventBase;
