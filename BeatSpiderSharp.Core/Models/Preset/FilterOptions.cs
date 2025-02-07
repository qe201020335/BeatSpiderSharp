using BeatSpiderSharp.Core.Models.Preset.Enums;
using Newtonsoft.Json;

namespace BeatSpiderSharp.Core.Models.Preset;

public class FilterOptions
{
    public Option<int?> UploaderId { get; set; } = new(null);
    public Option<string> UploaderName { get; set; } = new(string.Empty);
    public RangeOption<DateTimeOffset> UploadTime { get; set; } = new();
    public LogicOption<string> IncludeTags { get; set; } = new();
    public Option<IList<string>> ExcludeTags { get; set; } = new([]);
    public RangeOption<int> UpVotes { get; set; } = new();
    public RangeOption<float> UpVotePercentage { get; set; } = new();
    public RangeOption<int> DownVotes { get; set; } = new();
    public RangeOption<float> DownVotePercentage { get; set; } = new();
    public RangeOption<float> Rating { get; set; } = new();
    public Option FullSpread { get; set; } = new();
    public LogicOption<MCharacteristic> IncludeCharacteristics { get; set; } = new();
    public LogicOption<MDifficulty> IncludeDifficulties { get; set; } = new();
    public LogicOption<MMod> RequireMods { get; set; } = new();
    public Option<IList<MMod>> ExcludeMods { get; set; } = new([]);
    public RangeOption<float> Bpm { get; set; } = new();
    public RangeOption<int> Duration { get; set; } = new();
    public RangeOption<float> Njs { get; set; } = new();
    public RangeOption<float> Nps { get; set; } = new();
    public RangeOption<int> Notes { get; set; } = new();
    public RangeOption<int> Bombs { get; set; } = new();
    public RangeOption<int> Walls { get; set; } = new();
    public Option<IList<RankingStatus>> ScoreSaberRanking { get; set; } = new([]);
    public Option<IList<RankingStatus>> BeatLeaderRanking { get; set; } = new([]);
    public RangeOption<float> ScoreSaberStars { get; set; } = new();
    public RangeOption<float> BeatLeaderStars { get; set; } = new();
    public Option Chinese { get; set; } = new();

    public class Option
    {
        [JsonProperty(Order = -99)]
        public bool Enable { get; set; }
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

    public class LogicOption<T>() : Option<IList<T>>([])
    {
        public bool IsOr { get; set; }
    }
}