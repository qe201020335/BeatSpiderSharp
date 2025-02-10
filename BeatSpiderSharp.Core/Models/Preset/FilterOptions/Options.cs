using Newtonsoft.Json;

namespace BeatSpiderSharp.Core.Models.Preset.FilterOptions;

public class Option
{
    [JsonProperty(Order = -99)]
    public bool Enable { get; set; }

    public static implicit operator bool(Option option) => option.Enable;
}

public class Option<T> : Option
{
    [JsonProperty(Order = -89)]
    public T Filter { get; set; }

    public Option(T defaultValue)
    {
        Filter = defaultValue;
    }
}

public class RangeOption<T> : Option where T : struct, IComparable<T>
{
    public T? Min { get; set; }
    public T? Max { get; set; }

    public bool InRange(T value) => (!Min.HasValue || value.CompareTo(Min.Value) >= 0) &&
                                    (!Max.HasValue || value.CompareTo(Max.Value) <= 0);
}

public class LogicIncludeOption<T>() : Option<ISet<T>>(new HashSet<T>())
{
    public bool IsOr { get; set; }
        
    public bool SatisfiedBy(ICollection<T> values)
    {
        var required = Filter;
        return IsOr ? required.Any(values.Contains) : required.All(values.Contains);
    }
}