namespace Domain.User;

public record UsernameRegistry
{
    public Username Username { get; internal set; }
    public UserId UserId { get; internal set; }
}
