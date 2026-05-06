using Domain.Event;

namespace Domain.Album.Events;

public sealed record class AlbumRemovedEvent(AlbumId Id) : DomainEventBase(Id) { }
