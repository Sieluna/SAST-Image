using System.Text.Json;
using Client.Models;

namespace Client.Storage;

/// <summary>
/// Manages JWT token lifecycle using an <see cref="IStorage"/> backend.
/// Handles save, load, and clear of the access + refresh token pair.
/// </summary>
public sealed class JwtTokenStore
{
    private const string TokenKey = "sastimg.jwt";
    private readonly IStorage _storage;

    public JwtTokenStore(IStorage storage)
    {
        _storage = storage;
    }

    public async Task<JwtToken?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var json = await _storage.GetAsync(TokenKey, cancellationToken);
        if (json is null)
            return null;

        try
        {
            return JsonSerializer.Deserialize(json, ClientJsonContext.Default.JwtToken);
        }
        catch
        {
            return null;
        }
    }

    public Task SaveAsync(JwtToken token, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(token, ClientJsonContext.Default.JwtToken);
        return _storage.SetAsync(TokenKey, json, cancellationToken);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        return _storage.RemoveAsync(TokenKey, cancellationToken);
    }
}
