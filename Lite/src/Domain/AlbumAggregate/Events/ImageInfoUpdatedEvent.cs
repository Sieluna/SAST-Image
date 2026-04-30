using Domain.AlbumAggregate.ImageEntity;
using Domain.Event;

namespace Domain.AlbumAggregate.Events;

public sealed record class ImageInfoUpdatedEvent(ImageId Id, ImageTitle? Title, ImageTags? Tags)
    : IDomainEvent { }
