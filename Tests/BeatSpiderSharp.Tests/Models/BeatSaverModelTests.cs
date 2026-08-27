using System.Text.Json;
using BeatSpiderSharp.Models;
using BeatSpiderSharp.Models.BeatSaver;
using BeatSpiderSharp.Models.Enums;
using Xunit;

namespace BeatSpiderSharp.Tests.Models;

/// <summary>
/// Guards the two places the System.Text.Json models behave unlike the Newtonsoft ones they replaced.
/// </summary>
public class BeatSaverModelTests
{
    private static Song Deserialize(string json) =>
        JsonSerializer.Deserialize(json, BeatSaverJsonContext.Default.Song)!;

    /// <summary>
    /// System.Text.Json drops a property initializer when the JSON omits the property, where Newtonsoft reused
    /// the existing instance. BeatSaver omits <c>tags</c> and <c>collaborators</c> on most maps, so a plain
    /// <c>= []</c> would hand the filters a null and throw. The accessors coerce instead.
    /// </summary>
    [Fact]
    public void AbsentCollectionsAreEmptyNotNull()
    {
        var song = Deserialize("""{"id":"x","versions":[{"hash":"h"}]}""");

        Assert.Empty(song.Tags);
        Assert.Empty(song.Collaborators);
        Assert.Empty(song.LatestVersion.Diffs);
    }

    /// <summary>An explicit <c>null</c> must be coerced the same way as an absent property.</summary>
    [Fact]
    public void ExplicitlyNullCollectionsAreEmptyNotNull()
    {
        var song = Deserialize(
            """{"id":"x","tags":null,"collaborators":null,"versions":[{"hash":"h","diffs":null}]}""");

        Assert.Empty(song.Tags);
        Assert.Empty(song.Collaborators);
        Assert.Empty(song.LatestVersion.Diffs);
    }

    [Fact]
    public void MissingVersionsLeavesAnEmptyListRatherThanNull()
    {
        var song = Deserialize("""{"id":"x"}""");

        Assert.Empty(song.Versions);
        // LatestVersion is Versions.First(); ValidateBeatSaverSong is what gates this in the pipeline.
        Assert.False(BeatSpiderSong.ValidateBeatSaverSong(song));
    }

    /// <summary>
    /// The Newtonsoft reader used a global MissingMemberHandling.Error under DEBUG. System.Text.Json has no
    /// global equivalent, so each record carries a #if DEBUG [JsonUnmappedMemberHandling] attribute - which means
    /// a newly added model type can silently lose the check. This tracks the Models build configuration.
    /// </summary>
    [Fact]
    public void UnknownBeatSaverFieldThrowsInDebugAndIsIgnoredInRelease()
    {
        const string json = """{"id":"x","brandNewBeatSaverField":42}""";

#if DEBUG
        var ex = Assert.Throws<JsonException>(() => Deserialize(json));
        Assert.Contains("brandNewBeatSaverField", ex.Message);
#else
        Assert.Equal("x", Deserialize(json).Id);
#endif
    }

    [Theory]
    [InlineData(true, false, RankingStatus.Ranked)]
    [InlineData(false, true, RankingStatus.Qualified)]
    [InlineData(false, false, RankingStatus.Unranked)]
    public void RankingStatusIsDerivedFromTheFlags(bool ranked, bool qualified, RankingStatus expected)
    {
        var rankedFlag = ranked.ToString().ToLowerInvariant();
        var qualifiedFlag = qualified.ToString().ToLowerInvariant();
        var song = Deserialize("{\"id\":\"x\",\"ranked\":" + rankedFlag
                               + ",\"qualified\":" + qualifiedFlag
                               + ",\"blRanked\":" + rankedFlag
                               + ",\"blQualified\":" + qualifiedFlag + "}");

        Assert.Equal(expected, song.RankingStatus);
        Assert.Equal(expected, song.BlRankingStatus);
    }
}
