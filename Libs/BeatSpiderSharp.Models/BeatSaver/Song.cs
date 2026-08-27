using BeatSpiderSharp.Models.Enums;
using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

#if DEBUG
// Mirrors the old Newtonsoft MissingMemberHandling.Error: a new BeatSaver field throws in Debug, is ignored in Release.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
#endif
public record Song
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("uploader")]
    public User? Uploader { get; init; }

    [JsonPropertyName("metadata")]
    public Metadata? Metadata { get; init; }

    [JsonPropertyName("stats")]
    public Stats? Stats { get; init; }

    [JsonPropertyName("uploaded")]
    public DateTimeOffset? Uploaded { get; init; }

    [JsonPropertyName("automapper")]
    public bool Automapper { get; init; }

    [JsonPropertyName("ranked")]
    public bool Ranked { get; init; }

    [JsonPropertyName("qualified")]
    public bool Qualified { get; init; }

    [JsonIgnore]
    public RankingStatus RankingStatus
    {
        get
        {
            if (Ranked) return RankingStatus.Ranked;
            if (Qualified) return RankingStatus.Qualified;
            return RankingStatus.Unranked;
        }
    }

    // System.Text.Json does not keep a property initializer when the JSON omits the property, and it assigns
    // null for an explicit null - Newtonsoft reused the existing instance in both cases. BeatSaver omits these
    // for most maps, so coerce to empty here rather than making every consumer null-check.
    private readonly List<SongVersion> _versions = [];

    [JsonPropertyName("versions")]
    public List<SongVersion> Versions
    {
        get => _versions;
        init => _versions = value ?? [];
    }

    [JsonIgnore]
    public SongVersion LatestVersion => Versions.First();

    [JsonPropertyName("curator")]
    public User? Curator { get; init; }

    [JsonPropertyName("curatedAt")]
    public DateTimeOffset? CuratedAt { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }

    [JsonPropertyName("lastPublishedAt")]
    public DateTimeOffset? LastPublishedAt { get; init; }

    private readonly List<string> _tags = [];

    [JsonPropertyName("tags")]
    public List<string> Tags
    {
        get => _tags;
        init => _tags = value ?? [];
    }

    [JsonPropertyName("declaredAi")]
    public string? DeclaredAi { get; init; }

    [JsonPropertyName("blRanked")]
    public bool BlRanked { get; init; }

    [JsonPropertyName("blQualified")]
    public bool BlQualified { get; init; }

    [JsonIgnore]
    public RankingStatus BlRankingStatus
    {
        get
        {
            if (BlRanked) return RankingStatus.Ranked;
            if (BlQualified) return RankingStatus.Qualified;
            return RankingStatus.Unranked;
        }
    }

    [JsonPropertyName("bookmarked")]
    public bool Bookmarked { get; init; }

    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; init; }

    private readonly List<User> _collaborators = [];

    [JsonPropertyName("collaborators")]
    public List<User> Collaborators
    {
        get => _collaborators;
        init => _collaborators = value ?? [];
    }
}
