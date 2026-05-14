using Mediator;

namespace Query;

[Alias("QueryHandlerGrain")]
public interface IQueryHandlerGrain : IGrainWithGuidKey
{
    [Alias(nameof(QueryAsync))]
    public Task<TResult> QueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default
    );
}
