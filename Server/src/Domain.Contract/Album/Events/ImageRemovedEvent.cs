using Domain.Event;

namespace Domain.Album.Events;

public sealed record ImageRemovedEvent(AlbumId Id, ImageId ImageId) : DomainEventBase(Id);
