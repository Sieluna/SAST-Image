using System.Text.Json.Serialization;

namespace Client.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
