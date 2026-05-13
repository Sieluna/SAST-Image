using Domain.Album.Image;
using Domain.Event;

namespace Domain.Album.Events;

public sealed record AlbumImageUpdatedEvent(
    AlbumId Id,
    ImageId ImageId,
    ImageDescription? Description = null,
    ImageTags? Tags = null
) : DomainEventBase;
