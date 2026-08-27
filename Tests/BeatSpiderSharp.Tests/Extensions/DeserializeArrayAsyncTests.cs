using System.Text;
using System.Text.Json;
using BeatSpiderSharp.Extensions;
using BeatSpiderSharp.Models.BeatSaver;
using BeatSpiderSharp.Tests.TestHelpers;
using Xunit;

namespace BeatSpiderSharp.Tests.Extensions;

/// <summary>
/// The song cache is streamed, so the reader has to resume mid-value whenever a value spans two reads. These
/// tests replay the same payload at many chunk sizes to put that boundary at every byte offset in turn.
/// </summary>
public class DeserializeArrayAsyncTests
{
    /// <summary>1 puts a boundary between every pair of bytes; the large sizes cover the single-read case.</summary>
    public static TheoryData<int> ChunkSizes => [1, 2, 3, 7, 13, 64, 511, 4096, 1 << 20];

    private static async Task<List<Song>> ReadAsync(byte[] payload, int chunkSize, string[]? path = null,
        CancellationToken ct = default)
    {
        var songs = new List<Song>();
        await using var stream = new DripStream(payload, chunkSize);
        await foreach (var song in JsonExtensions.DeserializeArrayAsync(stream, BeatSaverJsonContext.Default.Song,
                           path ?? ["docs"], ct))
        {
            songs.Add(song);
        }

        return songs;
    }

    [Theory]
    [MemberData(nameof(ChunkSizes))]
    public async Task ReadsEveryElementWhateverTheChunkSize(int chunkSize)
    {
        var songs = await ReadAsync(CacheJson.Build(40), chunkSize);

        Assert.Equal(40, songs.Count);
        Assert.Equal(Enumerable.Range(0, 40).Select(i => i.ToString("x")), songs.Select(s => s.Id));
    }

    [Theory]
    [MemberData(nameof(ChunkSizes))]
    public async Task ReadsEveryFieldWhateverTheChunkSize(int chunkSize)
    {
        var songs = await ReadAsync(CacheJson.Build(40), chunkSize);

        for (var i = 0; i < songs.Count; i++)
        {
            var song = songs[i];
            Assert.Equal($"Song \u00e9{i}", song.Name);
            Assert.Equal(new string((char)('a' + i % 26), 30), song.Description);
            Assert.Equal($"mapper{i}", song.Uploader?.Name);
            Assert.Equal(200 + i, song.Metadata?.Duration);
            Assert.Equal(174.5f, song.Metadata?.Bpm);
            Assert.Equal(i * 3, song.Stats?.Upvotes);
            Assert.Equal(i % 2 == 0, song.Ranked);
            Assert.Equal(new DateTimeOffset(2024, 3, i % 9 + 1, 12, 34, 56, 789, TimeSpan.Zero), song.Uploaded);
            Assert.Equal(["Tech", "Dance"], song.Tags);
            Assert.Equal($"{i:x8}00000000000000000000000000000000", song.LatestVersion.Hash);

            var diff = Assert.Single(song.LatestVersion.Diffs);
            Assert.Equal(8.25f, diff.Stars);
            Assert.Equal(2, diff.ParitySummary?.Resets);
        }
    }

    [Fact]
    public async Task ReadsAnElementLargerThanTheReadBuffer()
    {
        // The reader's segments are 1 MiB, so a 3 MB description forces it to grow rather than spin.
        var songs = await ReadAsync(CacheJson.Build(2, 3_000_000), 8192);

        Assert.Equal(2, songs.Count);
        Assert.All(songs, s => Assert.Equal(3_000_000, s.Description?.Length));
    }

    [Theory]
    [MemberData(nameof(ChunkSizes))]
    public async Task SkipsANestedPropertyOfTheSameName(int chunkSize)
    {
        // CacheJson.Build puts an "info.docs" decoy ahead of the real "docs"; descending into it would either
        // throw or yield the decoy's strings.
        var songs = await ReadAsync(CacheJson.Build(3), chunkSize);

        Assert.Equal(3, songs.Count);
    }

    [Fact]
    public async Task WalksAMultiSegmentPath()
    {
        var payload = Encoding.UTF8.GetBytes(
            $$"""{"outer":{"decoy":{"inner":["nope"]},"inner":[{{CacheJson.Song(0)}}]},"date":1}""");

        var songs = await ReadAsync(payload, 16, ["outer", "inner"]);

        Assert.Equal("0", Assert.Single(songs).Id);
    }

    [Fact]
    public async Task YieldsNothingForAnEmptyArray()
    {
        Assert.Empty(await ReadAsync(CacheJson.Wrap(string.Empty), 4));
    }

    [Theory]
    [MemberData(nameof(ChunkSizes))]
    public async Task ThrowsWhenThePropertyIsMissing(int chunkSize)
    {
        var payload = "{\"info\":{\"total\":0},\"date\":1}"u8.ToArray();

        var ex = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(payload, chunkSize));
        Assert.Contains("Could not find array property", ex.Message);
    }

    [Fact]
    public async Task ThrowsWhenThePropertyIsNotAnArray()
    {
        var payload = "{\"docs\":{\"a\":1}}"u8.ToArray();

        var ex = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(payload, 4));
        Assert.Contains("not an array", ex.Message);
    }

    [Fact]
    public async Task ThrowsOnTruncatedInputRatherThanReturningAShortList()
    {
        var full = CacheJson.Build(40);

        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(full[..(full.Length / 2)], 128));
    }

    [Fact]
    public async Task ObservesCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(CacheJson.Build(40), 64, ct: cts.Token));
    }

    [Fact]
    public async Task StopsReadingWhenTheConsumerStopsEarly()
    {
        // A count limit abandons the enumeration part-way; the reader must not deadlock or throw on disposal.
        await using var stream = new DripStream(CacheJson.Build(200), 512);
        var taken = 0;

        await foreach (var _ in JsonExtensions.DeserializeArrayAsync(stream, BeatSaverJsonContext.Default.Song,
                           ["docs"]))
        {
            if (++taken == 5) break;
        }

        Assert.Equal(5, taken);
    }
}
