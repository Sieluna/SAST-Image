using Domain.Album;
using Domain.Album.Image;
using Domain.Category;
using Domain.User;

namespace Domain.Api;

public sealed record JwtTokenResponse(
    string AccessToken,
    string RefreshToken,
    long ExpireIn);

public sealed record AlbumResponse(
    AlbumId Id,
    string Title,
    string Description,
    UserId AuthorId,
    string AuthorName,
    CategoryId CategoryId,
    string CategoryName,
    string[] Tags,
    int SubscribeCount,
    long CreatedAt,
    long UpdatedAt);

public sealed record AlbumListResponse(
    AlbumResponse[] Albums,
    string? NextCursor);

public sealed record ImageResponse(
    ImageId Id,
    AlbumId AlbumId,
    string Title,
    UserId UploaderId,
    string UploaderName,
    string[] Tags,
    int Likes,
    bool Liked,
    long UploadedAt);

public sealed record CategoryResponse(
    CategoryId Id,
    string Name,
    string Description);

public sealed record UserProfileResponse(
    UserId Id,
    string Username,
    string Nickname,
    string Biography,
    long RegisteredAt);

public sealed record ErrorResponse(
    string Message,
    int Code);
