using Client.Storage;

namespace Client.Http;

public sealed record ClientOptions
{
    /// <summary>
    /// Base URL of the SAST-Image API (e.g. <c>http://localhost:5265</c>).
    /// Defaults to <c>http://localhost:5265</c>.
    /// </summary>
    public string BaseUrl { get; init; } = "http://localhost:5265";

    /// <summary>
    /// Storage backend for persisting JWT tokens.
    /// Defaults to <see cref="FileStorage"/> rooted at the assembly directory.
    /// </summary>
    public IStorage Storage { get; init; } = new MemoryStorage();

    /// <summary>
    /// Optional pre-configured HttpClient. When set, <see cref="BaseUrl"/> is ignored.
    /// </summary>
    public HttpClient? HttpClient { get; init; }
}
