using System.Net.Http.Json;
using System.Text.Json;
using Client.Models;

namespace Client.Apis;

public sealed class CategoryApi
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal CategoryApi(HttpClient http) => _http = http;

    /// <summary>POST /api/v1/categories (admin required)</summary>
    public async Task<long> CreateAsync(
        string name,
        string description,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/categories",
            new CreateCategoryRequest(name, description),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken);
    }

    /// <summary>PATCH /api/v1/categories/{id} (admin required)</summary>
    public async Task UpdateAsync(
        long categoryId,
        string? name = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"api/v1/categories/{categoryId}")
        {
            Content = JsonContent.Create(
                new UpdateCategoryRequest(name, description),
                options: JsonOptions),
        };

        var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccess();
    }

    /// <summary>GET /api/v1/categories</summary>
    public async Task<CategoryDto[]> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            "api/v1/categories",
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content
            .ReadFromJsonAsync<CategoryDto[]>(JsonOptions, cancellationToken)
            .ConfigureAwait(false) ?? [];
    }
}
