using Domain.Database;
using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;

namespace Domain.User;

[Reentrant]
[StatelessWorker]
internal sealed class UsernameManagerGrain(IDbContextFactory<DomainDbContext> factory)
    : IUsernameManagerGrain
{
    public async ValueTask<bool> Put(
        UserId userId,
        Username newUsername,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var usernameEntry = await context.Usernames.FirstOrDefaultAsync(
            u => u.UserId == userId,
            cancellationToken
        );

        if (usernameEntry is null)
            return false;

        var entries = await context
            .Usernames.Where(u =>
                u.UserId == userId || EF.Functions.ILike(u.Username.Value, newUsername.Value)
            )
            .ToArrayAsync(cancellationToken);

        if (entries.Length > 1 || (entries.Length == 1 && entries[0].UserId != userId))
            // The username is already taken by another user
            return false;

        if (entries.Length == 1 && entries[0].UserId == userId)
        {
            // User exists, trying to update a username that doesn't exist
            var user = entries[0];

            user.Username = newUsername;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Neither the user nor the username exists, create a new entry
        await context.Usernames.AddAsync(
            new() { UserId = userId, Username = newUsername },
            cancellationToken
        );
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async ValueTask<UserId?> Get(
        Username username,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var usernameEntry = await context
            .Usernames.AsNoTracking()
            .FirstOrDefaultAsync(
                u => EF.Functions.ILike(u.Username.Value, username.Value),
                cancellationToken
            );

        return usernameEntry?.UserId;
    }

    public async ValueTask<bool> Exists(
        Username username,
        CancellationToken cancellationToken = default
    )
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context
            .Usernames.AsNoTracking()
            .AnyAsync(u => EF.Functions.ILike(u.Username.Value, username.Value), cancellationToken);
    }
}
