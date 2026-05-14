using Domain.Category.Events;
using Domain.Event;

namespace Domain.Category;

internal sealed class CategoryGrain() : DomainGrain<CategoryState>, ICategoryGrain
{
    public async ValueTask<CategoryId> Create(CategoryName name, CategoryDescription description)
    {
        RaiseEvent(new CategoryCreatedEvent(Id, name, description));

        return Id;
    }

    public ValueTask Delete()
    {
        RaiseEvent(new CategoryDeletedEvent(Id));

        DeactivateOnIdle();

        return ValueTask.CompletedTask;
    }

    public ValueTask Update(CategoryName? name, CategoryDescription? description)
    {
        RaiseEvent(new CategoryUpdatedEvent(Id, name, description));

        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> Exists()
    {
        var exists = State.RecordExists;
        if (exists is false)
            DeactivateOnIdle();
        return ValueTask.FromResult(exists);
    }
}

internal sealed class CategoryState
    : DomainStateBase,
        IDomainEventApplicable<CategoryCreatedEvent>,
        IDomainEventApplicable<CategoryUpdatedEvent>,
        IDomainEventApplicable<CategoryDeletedEvent>
{
    public CategoryName Name { get; private set; }

    public override void Apply(DomainEventBase e)
    {
        switch (e)
        {
            case CategoryCreatedEvent e1:
                Apply(e1);
                break;
            case CategoryUpdatedEvent e2:
                Apply(e2);
                break;
            case CategoryDeletedEvent e3:
                Apply(e3);
                break;
            default:
                throw new NotSupportedException(
                    $"Event type {e.GetType().FullName} is not supported."
                );
        }
    }

    public void Apply(CategoryUpdatedEvent e)
    {
        Name = e.Name ?? Name;
    }

    public void Apply(CategoryDeletedEvent e)
    {
        RecordExists = false;
    }

    public void Apply(CategoryCreatedEvent e)
    {
        Name = e.Name;
        RecordExists = true;
    }
}
