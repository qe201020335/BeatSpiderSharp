using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record Metadata
{
    [JsonPropertyName("bpm")]
    public float? Bpm { get; init; }

    [JsonPropertyName("duration")]
    public int? Duration { get; init; }

    [JsonPropertyName("songName")]
    public string? SongName { get; init; }

    [JsonPropertyName("songSubName")]
    public string? SongSubName { get; init; }

    [JsonPropertyName("songAuthorName")]
    public string? SongAuthorName { get; init; }

    [JsonPropertyName("levelAuthorName")]
    public string? LevelAuthorName { get; init; }
}
