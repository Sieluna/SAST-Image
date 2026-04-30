using Domain.CategoryAggregate.Events;
using Domain.Event;

namespace Query.Categories.EventHandlers;

internal sealed class CategoryUpdatedEventHandler(ICategoryModelRepository repository)
    : IDomainEventHandler<CategoryUpdatedEvent>
{
    public async ValueTask Handle(CategoryUpdatedEvent e, CancellationToken cancellationToken)
    {
        var category = await repository.GetAsync(e.Id.Value, cancellationToken);

        if (e.Name is { Value: var name })
            category.Name = name;
        if (e.Description is { Value: var description })
            category.Description = description;
    }
}
