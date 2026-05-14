using Domain.Filters;

namespace Domain.Category;

[Alias("CategoryGrain")]
public interface ICategoryGrain : IGrainWithIntegerKey
{
    [AccessControl(Role.Admin)]
    [EnsureUniqueId]
    [AllowRecordNotExists]
    [Alias(nameof(Create))]
    ValueTask<CategoryId> Create(CategoryName name, CategoryDescription description);

    [AccessControl(Role.Admin)]
    [Alias(nameof(Update))]
    ValueTask Update(CategoryName? name, CategoryDescription? description);

    [AccessControl(Role.Admin)]
    [Alias(nameof(Delete))]
    ValueTask Delete();

    [Alias(nameof(Exists))]
    ValueTask<bool> Exists();
}
