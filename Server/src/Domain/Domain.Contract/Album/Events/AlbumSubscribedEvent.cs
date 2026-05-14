using Domain.Event;

namespace Domain.Album.Events;

public sealed record AlbumSubscribedEvent(AlbumId Id, Actor Actor) : DomainEventBase;
