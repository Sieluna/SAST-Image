using Domain.AlbumAggregate.AlbumEntity;
using Domain.AlbumAggregate.Exceptions;
using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace Domain.AlbumAggregate.Services;

internal sealed class CollaboratorsExistenceChecker(DomainDbContext context)
    : ICollaboratorsExistenceChecker
{
    private readonly DomainDbContext _context = context;

    public async Task CheckAsync(
        Collaborators collaborators,
        CancellationToken cancellationToken = default
    )
    {
        var userIdsInDb = await _context
            .Users.AsNoTracking()
            .Select(u => u.Id)
            .Where(u => collaborators.Value.Contains(u))
            .ToArrayAsync(cancellationToken);

        bool isAllExisting = collaborators.Value.All(userIdsInDb.Contains);

        if (isAllExisting == false)
            CollaboratorsNotFoundException.Throw(collaborators);
    }
}
