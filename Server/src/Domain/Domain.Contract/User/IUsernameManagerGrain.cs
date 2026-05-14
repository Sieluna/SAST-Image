namespace Domain.User;

[Alias("UsernameManagerGrain")]
public interface IUsernameManagerGrain : IGrainWithGuidKey
{
    [Alias(nameof(Put))]
    public ValueTask<bool> Put(
        UserId userId,
        Username newUsername,
        CancellationToken cancellationToken = default
    );

    [Alias(nameof(Get))]
    public ValueTask<UserId?> Get(Username username, CancellationToken cancellationToken = default);

    [Alias(nameof(Exists))]
    public ValueTask<bool> Exists(Username username, CancellationToken cancellationToken = default);
}
