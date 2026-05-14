using Mediator;

namespace Query;

[Orleans.Concurrency.StatelessWorker]
internal sealed class QueryHandlerGrain(IMediator mediator) : Grain, IQueryHandlerGrain
{
    public async Task<TResult> QueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(query, cancellationToken);
    }
}
