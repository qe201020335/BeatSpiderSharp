using System.Runtime.CompilerServices;

namespace BeatSpiderSharp.Tests.TestHelpers;

public static class AsyncSequence
{
    /// <summary>Wraps a sequence as <see cref="IAsyncEnumerable{T}"/>, honouring cancellation per element.</summary>
    public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in source)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }

    /// <summary>As <see cref="ToAsync{T}"/>, but records how many elements were pulled.</summary>
    public static async IAsyncEnumerable<T> ToAsyncCounting<T>(this IEnumerable<T> source, StrongBox<int> pulled,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in source)
        {
            ct.ThrowIfCancellationRequested();
            pulled.Value++;
            yield return item;
            await Task.Yield();
        }
    }
}
