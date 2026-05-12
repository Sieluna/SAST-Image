using System.Text.Json.Serialization;
using Client.Models;

namespace Client;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(SendRegistryCodeRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(ResetPasswordRequest))]
[JsonSerializable(typeof(ResetUsernameRequest))]
[JsonSerializable(typeof(CreateAlbumRequest))]
[JsonSerializable(typeof(UpdateAccessLevelRequest))]
[JsonSerializable(typeof(UpdateAlbumInfoRequest))]
[JsonSerializable(typeof(CreateCategoryRequest))]
[JsonSerializable(typeof(UpdateCategoryRequest))]
[JsonSerializable(typeof(UpdateImageRequest))]
[JsonSerializable(typeof(AddImageMetadata))]
[JsonSerializable(typeof(UpdateProfileRequest))]
[JsonSerializable(typeof(JwtToken))]
[JsonSerializable(typeof(AlbumDto))]
[JsonSerializable(typeof(AlbumDto[]))]
[JsonSerializable(typeof(RemovedAlbumDto))]
[JsonSerializable(typeof(RemovedAlbumDto[]))]
[JsonSerializable(typeof(ImageDto))]
[JsonSerializable(typeof(ImageDto[]))]
[JsonSerializable(typeof(CategoryDto))]
[JsonSerializable(typeof(CategoryDto[]))]
[JsonSerializable(typeof(UserProfileDto))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(long))]
public partial class ClientJsonContext : JsonSerializerContext
{
}
