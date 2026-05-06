namespace Domain.Album;

public interface ICategoryExistenceChecker
{
    ValueTask<bool> ExistsAsync(
        CategoryId categoryId,
        CancellationToken cancellationToken = default
    );
}

[Alias("category_not_found_exception")]
[GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]
public sealed class CategoryNotFoundException(CategoryId categoryId)
    : Exception($"Category with id {categoryId} not found.")
{
    public CategoryId CategoryId { get; } = categoryId;
}
