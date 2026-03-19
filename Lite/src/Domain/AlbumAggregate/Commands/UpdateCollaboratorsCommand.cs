using Domain.AlbumAggregate.AlbumEntity;
using Domain.AlbumAggregate.Services;
using Domain.Shared;
using Mediator;

namespace Domain.AlbumAggregate.Commands;

public sealed record class UpdateCollaboratorsCommand(
    AlbumId Album,
    Collaborators Collaborators,
    Actor Actor
) : ICommand { }

internal sealed class UpdateCollaboratorsCommandHandler(
    IAlbumRepository repository,
    ICollaboratorsExistenceChecker checker
) : ICommandHandler<UpdateCollaboratorsCommand>
{
    public async ValueTask<Unit> Handle(
        UpdateCollaboratorsCommand request,
        CancellationToken cancellationToken
    )
    {
        var album = await repository.GetAsync(request.Album, cancellationToken);

        await album.UpdateCollaborators(request, checker);

        return Unit.Value;
    }
}
