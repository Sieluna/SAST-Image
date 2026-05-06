global using AlbumId = long;
using Domain.Album.Events;
using Domain.Album.Image;
using Domain.Event;
using Domain.Filters;

namespace Domain.Album;

internal sealed class AlbumGrain(ICategoryExistenceChecker checker)
    : DomainGrain<AlbumState>,
        IAlbumGrain
{
    public async ValueTask<AlbumId> Create(
        AlbumTitle title,
        AlbumDescription description,
        AlbumTags tags,
        CategoryId categoryId
    )
    {
        if (await checker.ExistsAsync(categoryId) is false)
            throw new CategoryNotFoundException(categoryId);

        RaiseEvent(new AlbumCreatedEvent(Id, title, description, tags, categoryId, Actor));

        return Id;
    }

    public ValueTask Remove()
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();

        RaiseEvent(new AlbumRemovedEvent(Id));

        return ValueTask.CompletedTask;
    }

    public async ValueTask Update(
        AlbumTitle? title,
        AlbumDescription? description,
        AlbumTags? tags,
        CategoryId? categoryId
    )
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();
        if (categoryId is not null && await checker.ExistsAsync(categoryId.Value) is false)
            throw new CategoryNotFoundException(categoryId.Value);

        RaiseEvent(new AlbumUpdatedEvent(Id, title, description, tags, categoryId));
    }

    public ValueTask AddImage(ImageId id, ImageTitle title, ImageTags tags, ImageFile file)
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();
        if (State.Images.Contains(id))
            return ValueTask.CompletedTask;

        RaiseEvent(new ImageAddedEvent(Id, id, title, tags, file, Actor));
        return ValueTask.CompletedTask;
    }

    public ValueTask UpdateImage(ImageId id, ImageTitle? title, ImageTags? tags)
    {
        if (Actor.Id != State.Author && !Actor.IsAdmin is false)
            throw new ForbiddenException();

        if (State.Images.Contains(id) is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new ImageUpdatedEvent(Id, id, title, tags));
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveImage(ImageId id)
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();

        var exists = State.Images.Contains(id);
        if (exists is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new ImageRemovedEvent(Id, id));
        return ValueTask.CompletedTask;
    }

    public ValueTask Subscribe()
    {
        if (State.Subscribers.Contains(Actor.Id))
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumSubscribedEvent(Id, Actor));
        return ValueTask.CompletedTask;
    }

    public ValueTask Unsubscribe()
    {
        if (State.Subscribers.Contains(Actor.Id) is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumUnsubscribedEvent(Id, Actor));
        return ValueTask.CompletedTask;
    }

    public ValueTask LikeImage(ImageId id)
    {
        if (State.Images.Contains(id) is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new ImageLikedEvent(Id, id, Actor));
        return ValueTask.CompletedTask;
    }

    public ValueTask UnLikeImage(ImageId id)
    {
        if (State.Images.Contains(id) is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new ImageUnLikedEvent(Id, id, Actor));
        return ValueTask.CompletedTask;
    }
}

internal sealed class AlbumState : DomainStateBase, IDomainEventApplyable
{
    public UserId Author { get; private set; }
    public UserId[] Subscribers { get; private set; } = [];
    public ImageState[] Images { get; private set; } = [];

    public void Apply(DomainEventBase e)
    {
        (Author, Images, Subscribers, RecordExists) = e switch
        {
            AlbumCreatedEvent a => (a.Actor.Id, Images, Subscribers, true),
            AlbumUpdatedEvent => (Author, Images, Subscribers, RecordExists),
            AlbumRemovedEvent => (Author, Images, Subscribers, false),
            AlbumSubscribedEvent a => (Author, Images, Subscribers.Append(a.Actor.Id), true),
            AlbumUnsubscribedEvent a => (Author, Images, Subscribers.Filter(a.Actor.Id), true),
            //
            ImageUpdatedEvent => (Author, Images, Subscribers, RecordExists),
            ImageAddedEvent a => (Author, Images.Append(a.ImageId), Subscribers, RecordExists),
            ImageRemovedEvent a => (Author, Images.Filter(a.ImageId), Subscribers, RecordExists),
            _ => throw new InvalidOperationException($"Unknown event type: {e.GetType().Name}"),
        };
    }
}

file static class CollectionExtensions
{
    extension<T>(IEnumerable<T> elements)
        where T : IEquatable<T>
    {
        public T[] Filter(T element) => [.. elements.Where(e => !e.Equals(element))];

        public T[] Append(T element) => [.. elements, element];
    }

    extension(ImageState[] images)
    {
        public ImageState[] Filter(ImageId id) => Array.FindAll(images, i => i.Id != id);

        public ImageState[] Append(ImageId id) => [.. images, new ImageState { Id = id }];

        public bool Contains(ImageId id) => Array.Exists(images, i => i.Id == id);
    }
}
