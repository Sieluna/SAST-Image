using Domain.CategoryAggregate.Events;
using Domain.Event;

namespace Query.Categories.EventHandlers;

internal sealed class CategoryNameUpdatedEventHandler(ICategoryModelRepository repository)
    : IDomainEventHandler<CategoryNameUpdatedEvent>
{
    public async ValueTask Handle(CategoryNameUpdatedEvent e, CancellationToken cancellationToken)
    {
        var category = await repository.GetAsync(e.Id.Value, cancellationToken);

        category.Name = e.Name.Value;
    }
}
