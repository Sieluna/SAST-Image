namespace Client.Models;

public sealed record JwtToken(
    string AccessToken,
    string RefreshToken,
    long ExpireIn
);

public sealed record AlbumDto
{
    public long Id { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public long Author { get; init; }
    public long Category { get; init; }
    public string[] Tags { get; init; } = [];
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public AccessLevel AccessLevel { get; init; }
    public int SubscribeCount { get; init; }
}

public sealed record ImageDto
{
    public long Id { get; init; }
    public long AlbumId { get; init; }
    public long UploaderId { get; init; }
    public string Title { get; init; } = null!;
    public DateTime UploadedAt { get; init; }
    public string[] Tags { get; init; } = [];
    public int Likes { get; init; }
    public required RequesterInfoData Requester { get; init; }

    public sealed record RequesterInfoData(bool Liked);
}

public sealed record CategoryDto(long Id, string Name, string Description);

public sealed record UserProfileDto(long Id, string Username, string Nickname, string Biography);

public sealed record RemovedAlbumDto
{
    public long Id { get; init; }
    public string Title { get; init; } = null!;
    public long Category { get; init; }
    public AccessLevel AccessLevel { get; init; }
    public DateTime RemovedAt { get; init; }
}
