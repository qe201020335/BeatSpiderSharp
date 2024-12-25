namespace BeatSpiderSharp.Core.Utilities.Extensions;

public static class StringExtensions
{
    public static bool IsHex(this string str)
    {
        return str.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
    }
}