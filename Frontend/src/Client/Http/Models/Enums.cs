using System.Text.Json.Serialization;

namespace Client.Http.Models;

[JsonConverter(typeof(JsonStringEnumConverter<AccessLevel>))]
public enum AccessLevel : byte
{
    Private = 0,
    AuthReadOnly = 1,
    AuthReadWrite = 2,
    PublicReadOnly = 3,
    PublicReadWrite = 4,
}

public enum ImageKind
{
    Thumbnail = 0,
    Original = 1,
}
