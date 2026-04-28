using Domain.CategoryAggregate.Events;
using Domain.Event;

namespace Query.Categories.EventHandlers;

internal sealed class CategoryDescriptionUpdatedEventHandler(ICategoryModelRepository repository)
    : IDomainEventHandler<CategoryDescriptionUpdatedEvent>
{
    public async ValueTask Handle(
        CategoryDescriptionUpdatedEvent e,
        CancellationToken cancellationToken
    )
    {
        var category = await repository.GetAsync(e.Id.Value, cancellationToken);

        category.Description = e.Description.Value;
    }
}
