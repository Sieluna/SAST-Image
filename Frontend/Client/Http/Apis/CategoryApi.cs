using System.Net.Http.Json;
using Client.Http.Models;

namespace Client.Http.Apis;

public sealed class CategoryApi
{
    private readonly HttpClient _http;

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
            ClientJsonContext.Default.CreateCategoryRequest,
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content.ReadFromJsonAsync(
            ClientJsonContext.Default.Int64, cancellationToken);
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
                ClientJsonContext.Default.UpdateCategoryRequest),
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
            .ReadFromJsonAsync(ClientJsonContext.Default.CategoryDtoArray, cancellationToken)
            .ConfigureAwait(false) ?? [];
    }
}
