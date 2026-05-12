using System.Net.Http.Json;
using Client.Models;

namespace Client.Apis;

public sealed class UserApi
{
    private readonly HttpClient _http;

    internal UserApi(HttpClient http) => _http = http;

    /// <summary>PATCH /api/v1/users/profile (auth required)</summary>
    public async Task UpdateProfileAsync(
        string? nickname = null,
        string? biography = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            "api/v1/users/profile")
        {
            Content = JsonContent.Create(
                new UpdateProfileRequest(nickname, biography),
                ClientJsonContext.Default.UpdateProfileRequest),
        };

        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccess();
    }

    /// <summary>PUT /api/v1/users/avatar (auth required, multipart)</summary>
    public async Task UpdateAvatarAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        var response = await _http.PutAsync(
            "api/v1/users/avatar",
            content,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>PUT /api/v1/users/header (auth required, multipart)</summary>
    public async Task UpdateHeaderAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        var response = await _http.PutAsync(
            "api/v1/users/header",
            content,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>GET /api/v1/users/{id}/avatar — returns the raw image stream, or null if not found.</summary>
    public async Task<Stream?> GetAvatarAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"api/v1/users/{userId}/avatar",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccess();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    /// <summary>GET /api/v1/users/{id}/header — returns the raw image stream, or null if not found.</summary>
    public async Task<Stream?> GetHeaderAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"api/v1/users/{userId}/header",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccess();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    /// <summary>GET /api/v1/users/{id}/profile — returns the public profile, or null if not found.</summary>
    public async Task<UserProfileDto?> GetProfileAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"api/v1/users/{userId}/profile",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccess();
        return await response.Content
            .ReadFromJsonAsync(ClientJsonContext.Default.UserProfileDto, cancellationToken);
    }
}
