global using ImageId = long;
using Domain.Album.Events;

namespace Query.Images;

public sealed class ImageModel
{
    [Obsolete("For ORM", true)]
    private ImageModel() { }

    public long Id { get; }
    public string Title { get; internal set; } = null!;
    public long AlbumId { get; }
    public long AuthorId { get; }
    public long UploaderId { get; }
    public string[] Tags { get; internal set; } = [];
    public DateTime UploadedAt { get; } = DateTime.UtcNow;
    public List<LikeModel> Likes { get; } = null!;

    internal ImageModel(ImageAddedEvent e)
    {
        Id = e.ImageId;
        AlbumId = e.Id;
        Title = e.Title.Value;
        Tags = e.Tags.Value;
        UploadedAt = e.Timestamp;
        UploaderId = e.Uploader.Id;
    }
}

public sealed record class LikeModel(long Image, long User);
