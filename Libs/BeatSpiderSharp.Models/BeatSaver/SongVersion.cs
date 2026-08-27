using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record SongVersion
{
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    [JsonPropertyName("state")]
    public string? State { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    /**
     * How likely the map is auto generated.
     * Higher is less likely.
     * BeatSage maps are usually negative
     */
    [JsonPropertyName("sageScore")]
    public int? SageScore { get; init; }

    // System.Text.Json does not keep a property initializer when the JSON omits the property, and it assigns
    // null for an explicit null - Newtonsoft reused the existing instance in both cases. BeatSaver omits these
    // for most maps, so coerce to empty here rather than making every consumer null-check.
    private readonly List<Diff> _diffs = [];

    [JsonPropertyName("diffs")]
    public List<Diff> Diffs
    {
        get => _diffs;
        init => _diffs = value ?? [];
    }

    [JsonPropertyName("downloadURL")]
    public string? DownloadURL { get; init; }

    [JsonPropertyName("coverURL")]
    public string? CoverURL { get; init; }

    [JsonPropertyName("previewURL")]
    public string? PreviewURL { get; init; }

    [JsonPropertyName("key")]
    public string? Key { get; init; }
}
