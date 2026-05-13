namespace Client.Http.Models;

// ── Account ──────────────────────────────────────────────

public sealed record RegisterRequest(
    string Username,
    string Nickname,
    string Email,
    string Password,
    int Code
);

public sealed record SendRegistryCodeRequest(string Email);

public sealed record LoginRequest(string Username, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record ResetPasswordRequest(string OldPassword, string NewPassword);

public sealed record ResetUsernameRequest(string Username);

// ── Album ────────────────────────────────────────────────

public sealed record CreateAlbumRequest(
    string Title,
    string Description,
    long CategoryId,
    AccessLevel AccessLevel
);

public sealed record UpdateAccessLevelRequest(AccessLevel AccessLevel);

public sealed record UpdateAlbumInfoRequest(
    string? Title = null,
    string? Description = null,
    string[]? Tags = null
);

// ── Category ─────────────────────────────────────────────

public sealed record CreateCategoryRequest(string Name, string Description);

public sealed record UpdateCategoryRequest(
    string? Name = null,
    string? Description = null
);

// ── Image ────────────────────────────────────────────────

public sealed record UpdateImageRequest(
    string? Title = null,
    string[]? Tags = null
);

public sealed record AddImageMetadata(string Title, string[] Tags);

// ── User / Profile ───────────────────────────────────────

public sealed record UpdateProfileRequest(
    string? Nickname = null,
    string? Biography = null
);
