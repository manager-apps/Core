using System.Text.Json;

namespace Common;

/// <summary>
/// Shared JSON serializer options to ensure consistent serialization/deserialization.
/// </summary>
public static class JsonOptions
{
  /// <summary>
  /// Default options for serializing/deserializing events and messages.
  /// Uses camelCase naming policy and case-insensitive property matching.
  /// </summary>
  public static JsonSerializerOptions Default { get; } = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
  };
}

