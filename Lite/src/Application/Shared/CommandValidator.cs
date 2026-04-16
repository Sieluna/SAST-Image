using Mediator;

namespace Application.Shared;

public sealed class CommandValidator<TCommand, TResponse>(ICommandValidator<TCommand> validator)
    : MessagePreProcessor<TCommand, TResponse>
    where TCommand : IBaseCommand
{
    protected override async ValueTask Handle(TCommand message, CancellationToken cancellationToken)
    {
        await validator.ValidateAsync(message, cancellationToken);
    }
}

public interface ICommandValidator<TCommand>
    where TCommand : IBaseCommand
{
    public ValueTask ValidateAsync(TCommand command, CancellationToken cancellationToken);
}
