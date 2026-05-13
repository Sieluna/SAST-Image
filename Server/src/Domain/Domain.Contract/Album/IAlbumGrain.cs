using Domain.Album.Image;
using Domain.Category;
using Domain.File;
using Domain.Filters;

namespace Domain.Album;

[Alias("AlbumGrain")]
public interface IAlbumGrain : IGrainWithIntegerKey
{
    [AccessControl]
    [EnsureUniqueId]
    [AllowRecordNotExists]
    [Alias(nameof(Create))]
    public ValueTask<AlbumId> Create(
        AlbumTitle title,
        AlbumDescription description,
        AlbumTags tags,
        CategoryId categoryId
    );

    [AccessControl]
    [Alias(nameof(Update))]
    public ValueTask Update(
        AlbumTitle? title,
        AlbumDescription? description,
        AlbumTags? tags,
        CategoryId? categoryId
    );

    [AccessControl]
    [Alias(nameof(Remove))]
    public ValueTask Remove();

    [AccessControl]
    [Alias(nameof(Subscribe))]
    public ValueTask Subscribe();

    [AccessControl]
    [Alias(nameof(Unsubscribe))]
    public ValueTask Unsubscribe();

    [AccessControl]
    [Alias(nameof(AddImage))]
    public ValueTask AddImage(
        ImageId id,
        ImageDescription description,
        ImageTags tags,
        ImageFileKey file
    );

    [AccessControl]
    [Alias(nameof(UpdateImage))]
    public ValueTask UpdateImage(ImageId id, ImageDescription? description, ImageTags? tags);

    [AccessControl]
    [Alias(nameof(RemoveImage))]
    public ValueTask RemoveImage(ImageId id);
}
