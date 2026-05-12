using System.Net.Http.Json;
using Client.Http.Models;

namespace Client.Http.Apis;

public sealed class AlbumApi
{
    private readonly HttpClient _http;

    internal AlbumApi(HttpClient http) => _http = http;

    /// <summary>POST /api/v1/albums (auth required)</summary>
    public async Task<long> CreateAsync(
        CreateAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/albums",
            request,
            ClientJsonContext.Default.CreateAlbumRequest,
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content.ReadFromJsonAsync(
            ClientJsonContext.Default.Int64, cancellationToken);
    }

    /// <summary>POST /api/v1/albums/{id}/remove (auth required)</summary>
    public async Task RemoveAsync(long albumId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/remove",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/albums/{id}/restore (auth required)</summary>
    public async Task RestoreAsync(long albumId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/restore",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/albums/{id}/accessLevel (auth required)</summary>
    public async Task UpdateAccessLevelAsync(
        long albumId,
        AccessLevel accessLevel,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/v1/albums/{albumId}/accessLevel",
            new UpdateAccessLevelRequest(accessLevel),
            ClientJsonContext.Default.UpdateAccessLevelRequest,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>PATCH /api/v1/albums/{id}/info (auth required)</summary>
    public async Task UpdateInfoAsync(
        long albumId,
        string? title = null,
        string? description = null,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/v1/albums/{albumId}/info")
        {
            Content = JsonContent.Create(
                new UpdateAlbumInfoRequest(title, description, tags),
                ClientJsonContext.Default.UpdateAlbumInfoRequest),
        };

        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccess();
    }

    /// <summary>PUT /api/v1/albums/{id}/cover (auth required)</summary>
    public async Task UpdateCoverAsync(
        long albumId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        var response = await _http.PutAsync(
            $"api/v1/albums/{albumId}/cover",
            content,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>PUT /api/v1/albums/{id}/cover with no file (clears the cover).</summary>
    public async Task ClearCoverAsync(
        long albumId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsync(
            $"api/v1/albums/{albumId}/cover",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/albums/{id}/subscribe (auth required)</summary>
    public async Task SubscribeAsync(long albumId, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/subscribe",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>DELETE /api/v1/albums/{id}/subscribe (auth required)</summary>
    public async Task UnsubscribeAsync(long albumId, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(
            $"api/v1/albums/{albumId}/subscribe",
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>
    /// GET /api/v1/albums — cursor-paginated list (page size = 60).
    /// Pass the last album ID from the previous page as <paramref name="cursor"/>.
    /// Returns an empty array when there are no more pages.
    /// </summary>
    public async Task<AlbumDto[]> GetAlbumsAsync(
        long? categoryId = null,
        long? authorId = null,
        string? title = null,
        long? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (categoryId.HasValue) query.Add($"category={categoryId}");
        if (authorId.HasValue) query.Add($"author={authorId}");
        if (!string.IsNullOrEmpty(title)) query.Add($"title={Uri.EscapeDataString(title)}");
        if (cursor.HasValue) query.Add($"cursor={cursor}");

        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var response = await _http.GetAsync(
            $"api/v1/albums{qs}",
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content
            .ReadFromJsonAsync(ClientJsonContext.Default.AlbumDtoArray, cancellationToken)
            .ConfigureAwait(false) ?? [];
    }

    /// <summary>GET /api/v1/albums/removed (auth required)</summary>
    public async Task<RemovedAlbumDto[]> GetRemovedAlbumsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            "api/v1/albums/removed",
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content
            .ReadFromJsonAsync(ClientJsonContext.Default.RemovedAlbumDtoArray, cancellationToken)
            .ConfigureAwait(false) ?? [];
    }

    /// <summary>GET /api/v1/albums/{id}/cover — returns the raw image stream, or null if not found.</summary>
    public async Task<Stream?> GetCoverAsync(
        long albumId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"api/v1/albums/{albumId}/cover",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccess();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
