using Domain.Event;

namespace Domain.Album.Events;

public sealed record AlbumUpdatedEvent(
    AlbumId Id,
    AlbumTitle? Title,
    AlbumDescription? Description,
    AlbumTags? Tags,
    CategoryId? CategoryId
) : DomainEventBase(Id);
