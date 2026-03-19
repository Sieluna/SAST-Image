using Domain.CategoryAggregate.Commands;
using Domain.CategoryAggregate.Events;
using Domain.CategoryAggregate.Services;
using Domain.Entity;
using Domain.Shared;

namespace Domain.CategoryAggregate.CategoryEntity;

public sealed class Category : EntityBase<CategoryId>
{
    [Obsolete("For ORM", true)]
    private Category()
        : base(default) { }

    private Category(CreateCategoryCommand command)
        : base(CategoryId.GenerateNew())
    {
        _name = command.Name;
    }

    internal static async Task<CategoryId> CreateAsync(
        CreateCategoryCommand command,
        ICategoryNameUniquenessChecker checker,
        ICategoryRepository repository,
        CancellationToken cancellationToken
    )
    {
        await checker.CheckAsync(command.Name, cancellationToken);

        var newOne = new Category(command);

        await repository.AddAsync(newOne, cancellationToken);

        newOne.AddDomainEvent(
            new CategoryCreatedEvent(newOne.Id, command.Name, command.Description)
        );

        return newOne.Id;
    }

    private CategoryName _name;

    public void UpdateName(UpdateCategoryNameCommand command)
    {
        if (command.Actor.IsAdmin == false)
            throw new NoPermissionException();

        _name = command.Name;
        AddDomainEvent(new CategoryNameUpdatedEvent(Id, command.Name));
    }

    public void UpdateDescription(UpdateCategoryDescriptionCommand command)
    {
        if (command.Actor.IsAdmin == false)
            throw new NoPermissionException();
        AddDomainEvent(new CategoryDescriptionUpdatedEvent(Id, command.Description));
    }
}
