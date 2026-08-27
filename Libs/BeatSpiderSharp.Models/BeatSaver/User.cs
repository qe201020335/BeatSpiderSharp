using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record User
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("admin")]
    public bool Admin { get; init; }

    [JsonPropertyName("curator")]
    public bool Curator { get; init; }

    [JsonPropertyName("seniorCurator")]
    public bool SeniorCurator { get; init; }

    [JsonPropertyName("playlistUrl")]
    public string? PlaylistUrl { get; init; }

    [JsonPropertyName("curatorTab")]
    public bool CuratorTab { get; init; }

    [JsonPropertyName("verifiedMapper")]
    public bool VerifiedMapper { get; init; }

    [JsonPropertyName("uniqueSet")]
    public bool UniqueSet { get; init; }

    [JsonPropertyName("suspendedAt")]
    public DateTimeOffset? SuspendedAt { get; init; }
}
