using System.Net;
using System.Net.Http.Json;
using Client.Models;
using Client.Storage;

namespace Client;

/// <summary>
/// DelegatingHandler that injects the stored JWT access token into every request
/// and automatically refreshes the token pair when a 401 is received.
/// </summary>
internal sealed class AuthDelegatingHandler : DelegatingHandler
{
    private readonly JwtTokenStore _store;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AuthDelegatingHandler(JwtTokenStore store)
    {
        _store = store;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Inject access token if available.
        var token = await _store.LoadAsync(cancellationToken);
        if (token is not null)
        {
            request.Headers.Authorization = new("Bearer", token.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If not 401, or no stored token, return as-is.
        if (response.StatusCode != HttpStatusCode.Unauthorized || token is null)
            return response;

        // Attempt a single refresh.
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            // Re-read — another thread may have already refreshed.
            token = await _store.LoadAsync(cancellationToken);
            if (token is null)
                return response;

            var refreshRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "api/v1/account/refresh"
            )
            {
                Content = JsonContent.Create(
                    new RefreshTokenRequest(token.RefreshToken),
                    ClientJsonContext.Default.RefreshTokenRequest
                ),
            };

            var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);
            if (!refreshResponse.IsSuccessStatusCode)
            {
                await _store.ClearAsync(cancellationToken);
                return response;
            }

            var newToken = await refreshResponse.Content
                .ReadFromJsonAsync(ClientJsonContext.Default.JwtToken, cancellationToken)
                .ConfigureAwait(false);

            if (newToken is null)
            {
                await _store.ClearAsync(cancellationToken);
                return response;
            }

            await _store.SaveAsync(newToken, cancellationToken);

            // Clone and retry the original request with the new token.
            var retry = await CloneRequestAsync(request, cancellationToken);
            retry.Headers.Authorization = new("Bearer", newToken.AccessToken);
            return await base.SendAsync(retry, cancellationToken);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(bytes);
            foreach (var h in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        foreach (var h in request.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);

        return clone;
    }
}
