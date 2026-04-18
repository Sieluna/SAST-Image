using Domain.AlbumAggregate.ImageEntity;

namespace Domain.AlbumAggregate.AlbumEntity;

/// <summary>
/// Represents a cover image and its status as the latest image for an entity.
/// </summary>
/// <remarks>Use the static members <see cref="Default"/> and <see cref="UserCustomCover"/> to represent common
/// cover states. This record is immutable.</remarks>
/// <param name="Id">The identifier of the cover image. Can be null if no image is associated.</param>
/// <param name="IsLatestImage">Indicates whether the cover image is the latest image. Set to <see langword="true"/> if the image is the most
/// recent; otherwise, <see langword="false"/>.</param>
public sealed record class Cover(ImageId? Id, bool IsLatestImage)
{
    public static readonly Cover Default = new(null, true);
    public static readonly Cover UserCustomCover = new(null, false);
}
