namespace Query.Categories;

public interface ICategoryModelRepository
{
    public Task<CategoryModel> GetAsync(long id, CancellationToken cancellationToken);
    public Task AddAsync(CategoryModel model, CancellationToken cancellationToken);
}
