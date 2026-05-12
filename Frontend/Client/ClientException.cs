using System.Net;

namespace Client;

public class ClientException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ClientException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
