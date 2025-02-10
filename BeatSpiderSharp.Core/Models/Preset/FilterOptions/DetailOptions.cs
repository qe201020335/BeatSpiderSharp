using BeatSpiderSharp.Core.Models.Preset.Enums;

namespace BeatSpiderSharp.Core.Models.Preset.FilterOptions;

//TODO separate song and individual difficulty filters
public class DetailOptions
{
    public Option<int?> UploaderId { get; set; } = new(null);
    public Option<string> UploaderName { get; set; } = new(string.Empty);
    public RangeOption<DateTimeOffset> UploadTime { get; set; } = new();
    public LogicIncludeOption<string> IncludeTags { get; set; } = new();
    public Option<IList<string>> ExcludeTags { get; set; } = new([]);
    public RangeOption<int> UpVotes { get; set; } = new();
    public RangeOption<float> UpVotePercentage { get; set; } = new();
    public RangeOption<int> DownVotes { get; set; } = new();
    public RangeOption<float> DownVotePercentage { get; set; } = new();
    public RangeOption<float> Rating { get; set; } = new();
    public Option FullSpread { get; set; } = new();
    public LogicIncludeOption<MCharacteristic> IncludeCharacteristics { get; set; } = new();
    public LogicIncludeOption<MDifficulty> IncludeDifficulties { get; set; } = new();
    public LogicIncludeOption<MMod> RequireMods { get; set; } = new();
    public Option<IList<MMod>> ExcludeMods { get; set; } = new([]);
    public RangeOption<float> Bpm { get; set; } = new();
    public RangeOption<int> Duration { get; set; } = new();
    public RangeOption<float> Njs { get; set; } = new();
    public RangeOption<float> Nps { get; set; } = new();
    public RangeOption<int> Notes { get; set; } = new();
    public RangeOption<int> Bombs { get; set; } = new();
    public RangeOption<int> Walls { get; set; } = new();
    public Option<ISet<RankingStatus>> ScoreSaberRanking { get; set; } = new(new HashSet<RankingStatus>());
    public Option<ISet<RankingStatus>> BeatLeaderRanking { get; set; } = new(new HashSet<RankingStatus>());
    public RangeOption<float> ScoreSaberStars { get; set; } = new();
    public RangeOption<float> BeatLeaderStars { get; set; } = new();
    public Option Chinese { get; set; } = new();
}