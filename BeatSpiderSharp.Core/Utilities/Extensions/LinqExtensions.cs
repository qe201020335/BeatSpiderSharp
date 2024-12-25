namespace BeatSpiderSharp.Core.Utilities.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<TValue> SelectNotNull<TValue>(this IEnumerable<TValue?> source) where TValue : struct
    {
        return source.Where(value => value.HasValue).Select(value => value!.Value);
    }
}