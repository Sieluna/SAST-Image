using System.Net.Http.Json;
using System.Text.Json;
using Client.Models;

namespace Client.Apis;

public sealed class ImageApi
{
    private readonly HttpClient _http;

    internal ImageApi(HttpClient http) => _http = http;

    /// <summary>
    /// POST /api/v1/albums/{albumId}/add (auth required).
    /// The metadata JSON string is sent alongside the file as multipart/form-data.
    /// </summary>
    public async Task<long> AddImageAsync(
        long albumId,
        Stream fileStream,
        string fileName,
        string title,
        string[] tags,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        var metadata = JsonSerializer.Serialize(
            new AddImageMetadata(title, tags),
            ClientJsonContext.Default.AddImageMetadata);
        content.Add(new StringContent(metadata), "metadata");

        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/add",
            content,
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content.ReadFromJsonAsync(
            ClientJsonContext.Default.Int64, cancellationToken);
    }

    /// <summary>PATCH /api/v1/albums/{albumId}/images/{imageId} (auth required)</summary>
    public async Task UpdateAsync(
        long albumId,
        long imageId,
        string? title = null,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/v1/albums/{albumId}/images/{imageId}")
        {
            Content = JsonContent.Create(
                new UpdateImageRequest(title, tags),
                ClientJsonContext.Default.UpdateImageRequest),
        };

        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/albums/{albumId}/images/{imageId}/remove (auth required)</summary>
    public async Task RemoveAsync(
        long albumId,
        long imageId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/images/{imageId}/remove",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/albums/{albumId}/images/{imageId}/restore (auth required)</summary>
    public async Task RestoreAsync(
        long albumId,
        long imageId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/images/{imageId}/restore",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/albums/{albumId}/images/{imageId}/like (auth required)</summary>
    public async Task LikeAsync(
        long albumId,
        long imageId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"api/v1/albums/{albumId}/images/{imageId}/like",
            null,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>DELETE /api/v1/albums/{albumId}/images/{imageId}/like (auth required)</summary>
    public async Task UnlikeAsync(
        long albumId,
        long imageId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(
            $"api/v1/albums/{albumId}/images/{imageId}/like",
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>DELETE /api/v1/albums/{albumId}/images/{imageId} (auth required, permanent)</summary>
    public async Task DeleteAsync(
        long albumId,
        long imageId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(
            $"api/v1/albums/{albumId}/images/{imageId}",
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>
    /// GET /api/v1/images — cursor-paginated list (page size = 50).
    /// Pass the last image ID from the previous page as <paramref name="cursor"/>.
    /// Returns an empty array when there are no more pages.
    /// </summary>
    public async Task<ImageDto[]> GetImagesAsync(
        long? uploaderId = null,
        long? albumId = null,
        long? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (uploaderId.HasValue) query.Add($"uploader={uploaderId}");
        if (albumId.HasValue) query.Add($"album={albumId}");
        if (cursor.HasValue) query.Add($"page={cursor}");

        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var response = await _http.GetAsync(
            $"api/v1/images{qs}",
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content
            .ReadFromJsonAsync(ClientJsonContext.Default.ImageDtoArray, cancellationToken)
            .ConfigureAwait(false) ?? [];
    }

    /// <summary>
    /// GET /api/v1/images/{id}?kind= — returns the raw image stream, or null if not found.
    /// </summary>
    public async Task<Stream?> GetImageFileAsync(
        long imageId,
        ImageKind kind = ImageKind.Thumbnail,
        CancellationToken cancellationToken = default)
    {
        var kindStr = kind switch
        {
            ImageKind.Original => "Original",
            _ => "Thumbnail",
        };

        var response = await _http.GetAsync(
            $"api/v1/images/{imageId}?kind={kindStr}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccess();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    /// <summary>GET /api/v1/albums/{albumId}/images/removed (auth required)</summary>
    public async Task<ImageDto[]> GetRemovedImagesAsync(
        long albumId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"api/v1/albums/{albumId}/images/removed",
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content
            .ReadFromJsonAsync(ClientJsonContext.Default.ImageDtoArray, cancellationToken)
            .ConfigureAwait(false) ?? [];
    }
}
