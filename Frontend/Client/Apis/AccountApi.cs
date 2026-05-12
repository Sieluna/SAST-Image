using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Client.Models;
using Client.Storage;

namespace Client.Apis;

public sealed class AccountApi
{
    private readonly HttpClient _http;
    private readonly JwtTokenStore _store;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal AccountApi(HttpClient http, JwtTokenStore store)
    {
        _http = http;
        _store = store;
    }

    /// <summary>POST /api/v1/account/register</summary>
    public async Task<JwtToken> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/account/register",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccess();

        var token = (await response.Content
            .ReadFromJsonAsync<JwtToken>(JsonOptions, cancellationToken))!;

        await _store.SaveAsync(token, cancellationToken);
        return token;
    }

    /// <summary>POST /api/v1/account/register/code</summary>
    public async Task SendRegistryCodeAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/account/register/code",
            new SendRegistryCodeRequest(email),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/account/login</summary>
    public async Task<JwtToken> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/account/login",
            new LoginRequest(username, password),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccess();

        var token = (await response.Content
            .ReadFromJsonAsync<JwtToken>(JsonOptions, cancellationToken))!;

        await _store.SaveAsync(token, cancellationToken);
        return token;
    }

    /// <summary>POST /api/v1/account/refresh</summary>
    public async Task<JwtToken?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/account/refresh",
            new RefreshTokenRequest(refreshToken),
            JsonOptions,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccess();

        var token = (await response.Content
            .ReadFromJsonAsync<JwtToken>(JsonOptions, cancellationToken))!;

        await _store.SaveAsync(token, cancellationToken);
        return token;
    }

    /// <summary>POST /api/v1/account/reset/password (auth required)</summary>
    public async Task ResetPasswordAsync(
        string oldPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/account/reset/password",
            new ResetPasswordRequest(oldPassword, newPassword),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>POST /api/v1/account/reset/username (auth required)</summary>
    public async Task ResetUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/v1/account/reset/username",
            new ResetUsernameRequest(username),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccess();
    }

    /// <summary>GET /api/v1/account/username/check?username=</summary>
    public async Task<bool> CheckUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"api/v1/account/username/check?username={Uri.EscapeDataString(username)}",
            cancellationToken);

        response.EnsureSuccess();
        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
    }

    /// <summary>GET /api/v1/account/oauth/github — returns the redirect URL for the GitHub OAuth flow.</summary>
    public string GetGitHubOAuthUrl()
    {
        var baseUrl = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
        return $"{baseUrl}/api/v1/account/oauth/github";
    }

    /// <summary>GET /api/v1/account/oauth/github/link (auth required) — returns the link URL.</summary>
    public string GetGitHubLinkUrl()
    {
        var baseUrl = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
        return $"{baseUrl}/api/v1/account/oauth/github/link";
    }
}

internal static class HttpResponseMessageExtensions
{
    internal static void EnsureSuccess(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ClientException(
                response.StatusCode,
                $"API request failed ({response.StatusCode}): {response.RequestMessage?.RequestUri}"
            );
        }
    }
}
