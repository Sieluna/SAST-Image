using Domain.AlbumAggregate.AlbumEntity;
using Domain.Shared;
using Mediator;

namespace Domain.AlbumAggregate.Commands;

public sealed record class UpdateAlbumTitleCommand(AlbumId Album, AlbumTitle Title, Actor Actor)
    : ICommand { }

internal sealed class UpdateAlbumTitleCommandHandler(IAlbumRepository repository)
    : ICommandHandler<UpdateAlbumTitleCommand>
{
    public async ValueTask<Unit> Handle(
        UpdateAlbumTitleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var album = await repository.GetAsync(command.Album, cancellationToken);

        album.UpdateTitle(command);

        return Unit.Value;
    }
}
