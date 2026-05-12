using Client.Storage;
using Client.SignalR;
using Client.Http;

namespace Client;

public sealed class SastClient
{
    public ClientOptions Options { get; }

    public SastClient(string baseUrl, IStorage storage)
        => Options = new ClientOptions { BaseUrl = baseUrl, Storage = storage };

    public SastClient(ClientOptions options)
        => Options = options;

    public SignalRClient SignalR() => new(Options.BaseUrl, Options.Storage);
    public HttpRestClient Http() => new(Options);
}
