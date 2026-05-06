using Domain.Event;

namespace Domain.Album.Events;

public sealed record class AlbumUnsubscribedEvent(AlbumId Id, Actor Actor) : DomainEventBase(Id) { }
