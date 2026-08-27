using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record Diff
{
    [JsonPropertyName("njs")]
    public float? Njs { get; init; }

    [JsonPropertyName("offset")]
    public float? Offset { get; init; }

    [JsonPropertyName("notes")]
    public int? Notes { get; init; }

    [JsonPropertyName("bombs")]
    public int? Bombs { get; init; }

    [JsonPropertyName("obstacles")]
    public int? Obstacles { get; init; }

    [JsonPropertyName("nps")]
    public float? Nps { get; init; }

    /**
     * The length of the map in beats.
     */
    [JsonPropertyName("length")]
    public float? Length { get; init; }

    [JsonPropertyName("characteristic")]
    public string? Characteristic { get; init; }

    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; init; }

    [JsonPropertyName("events")]
    public int? Events { get; init; }

    [JsonPropertyName("chroma")]
    public bool Chroma { get; init; }

    [JsonPropertyName("me")]
    public bool Me { get; init; }

    [JsonPropertyName("ne")]
    public bool Ne { get; init; }

    [JsonPropertyName("cinema")]
    public bool Cinema { get; init; }

    [JsonPropertyName("seconds")]
    public float? Seconds { get; init; }

    [JsonPropertyName("paritySummary")]
    public ParitySummary? ParitySummary { get; init; }

    [JsonPropertyName("stars")]
    public float? Stars { get; init; }

    [JsonPropertyName("maxScore")]
    public int? MaxScore { get; init; }

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("blStars")]
    public float? BlStars { get; init; }

    [JsonPropertyName("environment")]
    public string? Environment { get; init; }

    [JsonPropertyName("vivify")]
    public bool Vivify { get; init; }
}
