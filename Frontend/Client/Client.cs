using Client.Storage;
using Client.SignalR;
using Client.Http;

namespace Client;

public sealed class SastClient
{
    private SignalRClient? _signalR;
    private HttpRestClient? _http;

    public ClientOptions Options { get; }

    public SastClient(string baseUrl, IStorage storage)
        => Options = new ClientOptions { BaseUrl = baseUrl, Storage = storage };

    public SastClient(ClientOptions options)
        => Options = options;

    public SignalRClient SignalR() => _signalR ??= new(Options.BaseUrl, Options.Storage);
    public HttpRestClient Http() => _http ??= new(Options);
}
