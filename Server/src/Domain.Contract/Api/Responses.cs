namespace Domain.Api;

public sealed record JwtTokenResponse(
    string AccessToken,
    string RefreshToken,
    long ExpireIn);

public sealed record AlbumResponse(
    long Id,
    string Title,
    string Description,
    long AuthorId,
    string AuthorName,
    long CategoryId,
    string CategoryName,
    string[] Tags,
    int SubscribeCount,
    long CreatedAt,
    long UpdatedAt);

public sealed record AlbumListResponse(
    AlbumResponse[] Albums,
    string? NextCursor);

public sealed record ImageResponse(
    long Id,
    long AlbumId,
    string Title,
    long UploaderId,
    string UploaderName,
    string[] Tags,
    int Likes,
    bool Liked,
    long UploadedAt);

public sealed record CategoryResponse(
    long Id,
    string Name,
    string Description);

public sealed record UserProfileResponse(
    long Id,
    string Username,
    string Nickname,
    string Biography,
    long RegisteredAt);

public sealed record ErrorResponse(
    string Message,
    int Code);
