using Client.Apis;
using Client.Storage;

namespace Client;

/// <summary>
/// Typed client for the SAST-Image API. Provides access to all API groups
/// and manages authentication state transparently via an <see cref="IStorage"/> backend.
/// </summary>
public sealed class Client : IDisposable
{
    private readonly HttpClient _http;
    private readonly JwtTokenStore _store;
    private readonly bool _ownsHttpClient;

    public AccountApi Account { get; }
    public AlbumApi Album { get; }
    public CategoryApi Category { get; }
    public ImageApi Image { get; }
    public UserApi User { get; }

    public Client(ClientOptions options)
    {
        _store = new JwtTokenStore(options.Storage);

        if (options.HttpClient is not null)
        {
            _http = options.HttpClient;
            _ownsHttpClient = false;
        }
        else
        {
            // Wire AuthDelegatingHandler → HttpClientHandler chain.
            var authHandler = new AuthDelegatingHandler(_store)
            {
                InnerHandler = new HttpClientHandler(),
            };
            _http = new HttpClient(authHandler)
            {
                BaseAddress = new Uri(options.BaseUrl.TrimEnd('/')),
            };
            _ownsHttpClient = true;
        }

        Account = new AccountApi(_http, _store);
        Album = new AlbumApi(_http);
        Category = new CategoryApi(_http);
        Image = new ImageApi(_http);
        User = new UserApi(_http);
    }

    /// <summary>
    /// Shortcut: base URL (default <c>http://localhost:5265</c>) + default <see cref="FileStorage"/>.
    /// </summary>
    public Client(string baseUrl = "http://localhost:5265")
        : this(new ClientOptions { BaseUrl = baseUrl }) { }

    /// <summary>
    /// Shortcut: default base URL + custom storage.
    /// </summary>
    public Client(IStorage storage)
        : this(new ClientOptions { Storage = storage }) { }

    /// <summary>
    /// Returns the current JWT token pair if stored, or null.
    /// </summary>
    public Task<Models.JwtToken?> GetTokenAsync(CancellationToken cancellationToken = default)
        => _store.LoadAsync(cancellationToken);

    /// <summary>
    /// Whether a JWT token is currently stored (does not validate expiry).
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
        => await _store.LoadAsync(cancellationToken) is not null;

    /// <summary>
    /// Manually clear stored tokens (log out).
    /// </summary>
    public Task LogoutAsync(CancellationToken cancellationToken = default)
        => _store.ClearAsync(cancellationToken);

    public void Dispose()
    {
        if (_ownsHttpClient)
            _http.Dispose();
    }
}
