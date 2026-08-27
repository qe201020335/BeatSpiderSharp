using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record Stats
{
    // Nothing reads these two, but they stay mapped so the DEBUG unmapped-member check does not trip on the
    // cached data, which still carries them.
    [JsonPropertyName("plays")]
    [Obsolete("BeatSaver does not keep track of it")]
    public int? Plays { get; init; }

    [JsonPropertyName("downloads")]
    [Obsolete("BeatSaver does not keep track of it.")]
    public int? Downloads { get; init; }

    [JsonPropertyName("upvotes")]
    public int? Upvotes { get; init; }

    [JsonPropertyName("downvotes")]
    public int? Downvotes { get; init; }

    [JsonPropertyName("score")]
    public float? Score { get; init; }

    [JsonPropertyName("reviews")]
    public int? Reviews { get; init; }

    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; init; }
}
