using System.Runtime.CompilerServices;
using BeatSpiderSharp.Extensions;
using BeatSpiderSharp.Tests.TestHelpers;
using Xunit;

namespace BeatSpiderSharp.Tests.Extensions;

/// <summary>
/// <see cref="LinqExtensions.TakeTopByAsync{TSource,TKey}"/> claims to equal
/// <c>OrderByDescending(key).Take(count)</c>, result for result. Nearly every test here is that claim checked
/// against the real thing, because a heap is not stable and the equivalence is the whole contract.
/// </summary>
public class TakeTopByAsyncTests
{
    private sealed record Item(int Id, float? Score);

    /// <summary>
    /// Few distinct scores means heavy ties, which is where an unstable selection diverges. Comparing by Id
    /// rather than by score makes a tie broken the wrong way a failure.
    /// </summary>
    private static List<Item> Items(int size, int distinctScores, bool includeNulls, int seed = 20260827)
    {
        var rng = new Random(seed);
        var items = new List<Item>(size);
        for (var i = 0; i < size; i++)
        {
            float? score = includeNulls && rng.Next(4) == 0 ? null : rng.Next(distinctScores) / 10f;
            items.Add(new Item(i, score));
        }

        return items;
    }

    [Theory]
    // size, distinctScores, includeNulls, count
    [InlineData(0, 5, false, 10)]
    [InlineData(1, 1, false, 1)]
    [InlineData(5, 1, false, 3)]
    [InlineData(50, 3, true, 7)]
    [InlineData(200, 7, true, 100)]
    [InlineData(1000, 4, false, 100)]
    [InlineData(1000, 1, true, 100)]
    [InlineData(997, 500, true, 13)]
    [InlineData(100, 2, true, 100)]
    [InlineData(100, 2, true, 105)]
    [InlineData(100, 2, true, 1)]
    public async Task MatchesOrderByDescendingThenTake(int size, int distinctScores, bool includeNulls, int count)
    {
        var items = Items(size, distinctScores, includeNulls);

        var expected = items.OrderByDescending(x => x.Score).Take(count).Select(x => x.Id);
        var actual = await items.ToAsync().TakeTopByAsync(count, x => x.Score);

        Assert.Equal(expected, actual.Select(x => x.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task NonPositiveCountYieldsNothingWithoutEnumerating(int count)
    {
        var pulled = new StrongBox<int>(0);

        var result = await Items(50, 5, false).ToAsyncCounting(pulled).TakeTopByAsync(count, x => x.Score);

        Assert.Empty(result);
        Assert.Equal(0, pulled.Value);
    }

    [Fact]
    public async Task EnumeratesTheSourceExactlyOnce()
    {
        var pulled = new StrongBox<int>(0);
        var items = Items(500, 10, true);

        await items.ToAsyncCounting(pulled).TakeTopByAsync(10, x => x.Score);

        Assert.Equal(items.Count, pulled.Value);
    }

    [Fact]
    public async Task NullKeysSortLast()
    {
        // Only enough non-null scores to fill part of the result, so nulls must be pulled in for the remainder
        // and must land at the end.
        var items = new List<Item>
        {
            new(0, null), new(1, 0.5f), new(2, null), new(3, 0.9f), new(4, null)
        };

        var result = await items.ToAsync().TakeTopByAsync(5, x => x.Score);

        Assert.Equal([3, 1, 0, 2, 4], result.Select(x => x.Id));
    }

    [Fact]
    public async Task TiesKeepSourceOrder()
    {
        var items = Enumerable.Range(0, 20).Select(i => new Item(i, 1f)).ToList();

        var result = await items.ToAsync().TakeTopByAsync(5, x => x.Score);

        Assert.Equal([0, 1, 2, 3, 4], result.Select(x => x.Id));
    }

    [Fact]
    public async Task HonoursACustomComparer()
    {
        var numbers = Enumerable.Range(0, 100).Select(i => i * 37 % 100).ToList();
        var reversed = Comparer<int>.Create((a, b) => b.CompareTo(a));

        var expected = numbers.OrderByDescending(x => x, reversed).Take(5);
        var actual = await numbers.ToAsync().TakeTopByAsync(5, x => x, reversed);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ThrowsOnNullArguments()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            LinqExtensions.TakeTopByAsync<Item, float?>(null!, 1, x => x.Score));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Items(5, 2, false).ToAsync().TakeTopByAsync(1, (Func<Item, float?>)null!));
    }

    [Fact]
    public async Task ObservesCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Items(50, 5, false).ToAsync(cts.Token).TakeTopByAsync(10, x => x.Score, ct: cts.Token));
    }
}
