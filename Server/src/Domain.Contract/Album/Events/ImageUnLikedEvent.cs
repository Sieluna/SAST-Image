using Domain.Event;

namespace Domain.Album.Events;

public sealed record class ImageUnLikedEvent(AlbumId Id, ImageId ImageId, Actor Actor)
    : DomainEventBase(Id);
