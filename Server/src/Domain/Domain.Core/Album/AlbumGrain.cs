using Domain.Album.Events;
using Domain.Album.Image;
using Domain.Category;
using Domain.Event;
using Domain.File;
using Domain.Filters;
using Domain.User;

namespace Domain.Album;

internal sealed class AlbumGrain : DomainGrain<AlbumState>, IAlbumGrain
{
    public async ValueTask<AlbumId> Create(
        AlbumTitle title,
        AlbumDescription description,
        AlbumTags tags,
        CategoryId categoryId
    )
    {
        if (await GrainFactory.GetGrain<ICategoryGrain>(categoryId.Value).Exists() is false)
            throw new CategoryNotFoundException(categoryId);

        RaiseEvent(new AlbumCreatedEvent(new(Id), title, description, tags, categoryId, Actor));

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
        if (
            categoryId is { } id
            && await GrainFactory.GetGrain<ICategoryGrain>(id.Value).Exists() is false
        )
            throw new CategoryNotFoundException(id);

        RaiseEvent(new AlbumUpdatedEvent(Id, title, description, tags, categoryId));
    }

    public ValueTask AddImage(
        ImageId id,
        ImageDescription description,
        ImageTags tags,
        ImageFileKey file
    )
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();
        if (State.Images.Contains(id))
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumImageAddedEvent(Id, id, description, tags, file, Actor));
        return ValueTask.CompletedTask;
    }

    public ValueTask UpdateImage(ImageId id, ImageDescription? description, ImageTags? tags)
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();

        if (State.Images.Contains(id) is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumImageUpdatedEvent(Id, id, description, tags));
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveImage(ImageId id)
    {
        if (Actor.Id != State.Author && Actor.IsAdmin is false)
            throw new ForbiddenException();

        var exists = State.Images.Contains(id);
        if (exists is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumImageRemovedEvent(Id, id));
        return ValueTask.CompletedTask;
    }

    public ValueTask Subscribe()
    {
        if (State.Subscribes.Contains(Actor.Id))
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumSubscribedEvent(Id, Actor));
        return ValueTask.CompletedTask;
    }

    public ValueTask Unsubscribe()
    {
        if (State.Subscribes.Contains(Actor.Id) is false)
            return ValueTask.CompletedTask;

        RaiseEvent(new AlbumUnsubscribedEvent(Id, Actor));
        return ValueTask.CompletedTask;
    }
}

internal sealed class AlbumState
    : DomainStateBase,
        IDomainEventApplicable<AlbumCreatedEvent>,
        IDomainEventApplicable<AlbumUpdatedEvent>,
        IDomainEventApplicable<AlbumRemovedEvent>,
        IDomainEventApplicable<AlbumImageAddedEvent>,
        IDomainEventApplicable<AlbumImageUpdatedEvent>,
        IDomainEventApplicable<AlbumImageRemovedEvent>,
        IDomainEventApplicable<AlbumSubscribedEvent>,
        IDomainEventApplicable<AlbumUnsubscribedEvent>
{
    public UserId Author { get; private set; }
    public List<UserId> Subscribes { get; private set; } = [];
    public List<ImageState> Images { get; private set; } = [];

    public override void Apply(DomainEventBase e)
    {
        switch (e)
        {
            case AlbumCreatedEvent createdEvent:
                Apply(createdEvent);
                break;
            case AlbumUpdatedEvent updatedEvent:
                Apply(updatedEvent);
                break;
            case AlbumRemovedEvent removedEvent:
                Apply(removedEvent);
                break;
            case AlbumImageAddedEvent imageAddedEvent:
                Apply(imageAddedEvent);
                break;
            case AlbumImageUpdatedEvent imageUpdatedEvent:
                Apply(imageUpdatedEvent);
                break;
            case AlbumImageRemovedEvent imageRemovedEvent:
                Apply(imageRemovedEvent);
                break;
            case AlbumSubscribedEvent subscribedEvent:
                Apply(subscribedEvent);
                break;
            case AlbumUnsubscribedEvent unsubscribedEvent:
                Apply(unsubscribedEvent);
                break;
            default:
                throw new NotSupportedException(
                    $"Event type {e.GetType().FullName} is not supported."
                );
        }
    }

    public void Apply(AlbumImageRemovedEvent e)
    {
        var index = Images.FindIndex(i => i.Id == e.ImageId);
        Images.RemoveAt(index);
    }

    public void Apply(AlbumImageUpdatedEvent e)
    {
        // Do nothing.
    }

    public void Apply(AlbumImageAddedEvent e)
    {
        Images.Add(new ImageState { Id = e.ImageId });
    }

    public void Apply(AlbumRemovedEvent e)
    {
        RecordExists = false;
    }

    public void Apply(AlbumUpdatedEvent e)
    {
        // Do nothing.
    }

    public void Apply(AlbumCreatedEvent e)
    {
        Author = e.Actor.Id;
        Images = [];
        Subscribes = [];
        RecordExists = true;
    }

    public void Apply(AlbumUnsubscribedEvent e)
    {
        var index = Subscribes.FindIndex(i => i == e.Actor.Id);
        Images.RemoveAt(index);
    }

    public void Apply(AlbumSubscribedEvent e)
    {
        Subscribes.Add(e.Actor.Id);
    }
}

file static class CollectionExtensions
{
    extension(List<ImageState> images)
    {
        public bool Contains(ImageId id) => images.Exists(i => i.Id == id);
    }
}
