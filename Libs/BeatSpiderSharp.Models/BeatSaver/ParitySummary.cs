using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record ParitySummary
{
    [JsonPropertyName("errors")]
    public int? Errors { get; init; }

    [JsonPropertyName("warns")]
    public int? Warns { get; init; }

    [JsonPropertyName("resets")]
    public int? Resets { get; init; }
}
