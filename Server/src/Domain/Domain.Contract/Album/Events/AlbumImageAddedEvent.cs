using Domain.Album.Image;
using Domain.Event;
using Domain.File;

namespace Domain.Album.Events;

public sealed record AlbumImageAddedEvent(
    AlbumId Id,
    ImageId ImageId,
    ImageDescription Description,
    ImageTags Tags,
    ImageFileKey File,
    Actor Uploader
) : DomainEventBase;
