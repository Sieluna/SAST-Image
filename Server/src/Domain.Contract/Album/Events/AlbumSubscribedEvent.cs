using Domain.Event;

namespace Domain.Album.Events;

public sealed record class AlbumSubscribedEvent(AlbumId Id, Actor Actor) : DomainEventBase(Id) { }
