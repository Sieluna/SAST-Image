namespace Domain.Api;

public sealed record LoginRequest(string Username);

public sealed record RegisterRequest(
    string Username,
    string Nickname,
    string Biography);

public sealed record CreateAlbumRequest(
    string Title,
    string Description,
    string[] Tags,
    long CategoryId);

public sealed record UpdateAlbumRequest(
    string? Title,
    string? Description,
    string[]? Tags,
    long? CategoryId);

public sealed record AddImageRequest(
    string Title,
    string[] Tags,
    byte[] FileBytes);

public sealed record CreateCategoryRequest(
    string Name,
    string Description);

public sealed record UpdateCategoryRequest(
    string? Name,
    string? Description);

public sealed record UpdateProfileRequest(
    string? Username,
    string? Nickname,
    string? Biography);
