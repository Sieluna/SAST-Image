using Domain.Album.Image;
using Domain.Event;

namespace Domain.Album.Events;

public sealed record ImageUpdatedEvent(
    AlbumId Id,
    ImageId ImageId,
    ImageTitle? Title,
    ImageTags? Tags
) : DomainEventBase(Id);
