global using AlbumId = long;
using Domain.Album.Events;
using Query.Images;

namespace Query.Albums;

public sealed class AlbumModel
{
    [Obsolete("For ORM", true)]
    private AlbumModel() { }

    internal AlbumModel(AlbumCreatedEvent e)
    {
        Id = e.Id;
        Title = e.Title.Value;
        Description = e.Description.Value;
        AuthorId = e.Actor.Id;
        CategoryId = e.CategoryId;
    }

    public long Id { get; }
    public string Title { get; internal set; } = null!;
    public string Description { get; internal set; } = null!;
    public long AuthorId { get; private set; }
    public long CategoryId { get; internal set; }
    public string[] Tags { get; internal set; } = [];
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; internal set; } = DateTime.UtcNow;
    public List<SubscribeModel> Subscribes { get; } = null!;
    public List<ImageModel> Images { get; } = null!;
}

public sealed record class SubscribeModel(long Album, long User);
