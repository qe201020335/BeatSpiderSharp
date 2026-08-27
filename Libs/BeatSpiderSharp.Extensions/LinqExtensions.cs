namespace BeatSpiderSharp.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<TValue> SelectNotNull<TValue>(this IEnumerable<TValue?> source) where TValue : struct
    {
        return source.Where(value => value.HasValue).Select(value => value!.Value);
    }

    public static IEnumerable<TValue> SelectNotNull<TValue>(this IEnumerable<TValue?> source) where TValue : class
    {
        return source.Where(value => value is not null).Cast<TValue>();
    }

    /// <summary>
    /// Returns the <paramref name="count"/> highest-keyed elements, in descending key order, without ordering the
    /// rest. Matches <c>OrderByDescending(keySelector).Take(count)</c> exactly, ties included; a null key sorts
    /// last, and a non-positive <paramref name="count"/> yields nothing.
    /// </summary>
    /// <remarks>
    /// Cheaper than sorting when <paramref name="count"/> is small - not because of the sort, but because
    /// <c>OrderByDescending</c> must buffer the whole source, holding every element alive at once.
    /// </remarks>
    public static async Task<TSource[]> TakeTopByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source,
        int count, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        // Match Take(0), which yields nothing without touching the source.
        if (count <= 0) return [];

        var keyComparer = comparer ?? Comparer<TKey>.Default;

        // Orders keys worst-first, so the element that should lose its place is always the one at the root: the
        // lower key loses, and on equal keys the element that appeared later in the source loses.
        var worstFirst = Comparer<(TKey Key, int Index)>.Create((x, y) =>
        {
            var byKey = keyComparer.Compare(x.Key, y.Key);
            return byKey != 0 ? byKey : y.Index.CompareTo(x.Index);
        });

        // EnqueueDequeue does the comparison for us: an element no better than the root is handed straight back.
        var heap = new PriorityQueue<TSource, (TKey Key, int Index)>(count + 1, worstFirst);
        var index = 0;

        await foreach (var item in source.WithCancellation(ct))
        {
            var key = (keySelector(item), index++);
            if (heap.Count < count)
            {
                heap.Enqueue(item, key);
            }
            else
            {
                heap.EnqueueDequeue(item, key);
            }
        }

        // A heap is unordered below its root, so put the survivors into descending key order.
        var survivors = new List<(TSource Item, (TKey Key, int Index) Key)>(heap.Count);
        while (heap.TryDequeue(out var item, out var key))
        {
            survivors.Add((item, key));
        }

        survivors.Sort((a, b) => worstFirst.Compare(b.Key, a.Key));
        return survivors.Select(entry => entry.Item).ToArray();
    }
}
