using System.Text.Json.Serialization;

namespace Server.Api.Features.Auth;

public record GoogleLoginRequest(string Credential);

public record AuthTokenResponse(string Token, string Name, string Email, string? AvatarUrl);

internal record GoogleTokenInfo
{
  [JsonPropertyName("sub")] public string Sub { get; init; } = null!;
  [JsonPropertyName("email")] public string Email { get; init; } = null!;
  [JsonPropertyName("name")] public string Name { get; init; } = null!;
  [JsonPropertyName("picture")] public string? Picture { get; init; }
  [JsonPropertyName("aud")] public string Aud { get; init; } = null!;
}
