using Domain.Event;

namespace Domain.Album.Events;

public sealed record AlbumUnsubscribedEvent(AlbumId Id, Actor Actor) : DomainEventBase;
