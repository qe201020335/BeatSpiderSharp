using System.Text.RegularExpressions;
using BeatSpiderSharp.Core.Models.Preset;
using BeatSpiderSharp.Core.Models.Preset.Enums;
using Newtonsoft.Json;
using Serilog;
using BeatSpiderSharp.Core.Utilities.Extensions;

namespace BeatSpiderSharp.Core.Legacy;

public static class LegacyPresetLoader
{
    //beatsaver.com/profile/58338
    private static readonly Regex MapperUrlRegex = new(@"beatsaver.com/profile/(\d+)", RegexOptions.Compiled);

    private static readonly JsonSerializer LegacySerializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
#if DEBUG
        MissingMemberHandling = MissingMemberHandling.Error
#endif
    });

    public static LegacyPreset? LoadLegacyPreset(string path)
    {
        Log.Information("Loading legacy preset from {Path}", path);
        return LegacySerializer.DeserializeObject<LegacyPreset>(path);
    }

    public static void SaveLegacyPreset(LegacyPreset preset, string path)
    {
        Log.Information("Writing legacy preset to {Path}", path);
        LegacySerializer.Serialize(preset, path);
    }

    public static Preset ConvertToPreset(this LegacyPreset legacyPreset, string name)
    {
        Log.Information("Converting legacy preset to new preset: {Name}", name);
        WarnUnsupported(legacyPreset);
        var options = ConvertFilterOptions(legacyPreset.SongFilter);
        var output = new OutputConfig
        {
            LimitSongs = legacyPreset.Limits.Count.Enable,
            MaxSongs = legacyPreset.Limits.Count.Content,
            SavePlaylist = legacyPreset.Output.Playlist.Enable,
            PostProcessPlaylist = true,
            PlaylistPath = legacyPreset.Output.Playlist.Path,
            DownloadSongs = legacyPreset.Output.Songs.Enable,
            DownloadPath = legacyPreset.Output.Songs.Path,
            SkipExisting = true,
            ExistingSongPaths = legacyPreset.LocalSong.LocalSongSkipPaths.ToList(),
            CopyLocalSongs = true,
            LocalSongPaths = legacyPreset.LocalSong.LocalSongPaths.ToList()
        };
        var input = new InputConfig
        {
            Source = SongInputSource.SongDetailsCache,
            Playlists = string.IsNullOrWhiteSpace(legacyPreset.PlaylistInput.Path)
                ? []
                : [legacyPreset.PlaylistInput.Path],
            ManualInput = legacyPreset.ManualSongInput.Songs.ToList()
        };
        Log.Debug("Legacy preset input source: {Source}", legacyPreset.SongSource);
        switch (legacyPreset.SongSource)
        {
            case LegacyPreset.DataSource.LocalCache:
                input.Source = SongInputSource.SongDetailsCache;
                break;
            case LegacyPreset.DataSource.Playlist:
                input.Source = SongInputSource.Playlists;
                break;
            case LegacyPreset.DataSource.ManualInput:
                input.Source = SongInputSource.ManualInput;
                break;
            case LegacyPreset.DataSource.BeastSaber:
                Log.Warning("BeastSaber source is not supported!");
                input.Source = SongInputSource.SongDetailsCache;
                break;
            case LegacyPreset.DataSource.Mapper:
                MergeMapperSetting(options, legacyPreset.Mapper);
                input.Source = SongInputSource.SongDetailsCache;
                break;
            case LegacyPreset.DataSource.ScoreSaber:
                MergeScoreSaverSetting(options, legacyPreset.ScoreSaber);
                input.Source = SongInputSource.SongDetailsCache;
                break;
            case LegacyPreset.DataSource.BeatSaver:
                MergeBeatSaverSetting(options, legacyPreset.BeatSaver);
                input.Source = SongInputSource.SongDetailsCache;
                break;
            default:
                Log.Warning("Unknown song source from legacy preset: {Source}", legacyPreset.SongSource);
                input.Source = SongInputSource.SongDetailsCache;
                break;
        }

        var preset = new Preset
        {
            Name = name,
            Author = Environment.UserName,
            Description = $"该歌单由免费工具 BeatSpider (BeatSpiderSharp) 生成。\n\n" +
                          $"源项目地址（已停止更新）：https://github.com/WGzeyu/BeatSpider\n" + 
                          $"重制版项目地址：https://github.com/qe201020335/BeatSpiderSharp",
            Input = input,
            Output = output,
            DetailFilters = [options]
        };

#if DEBUG
        Log.Debug("Legacy preset: {@LegacyPreset}", legacyPreset);
        Log.Debug("Converted preset: {@NewPreset}", preset);
#endif
        return preset;
    }
    
    private static void WarnUnsupported(LegacyPreset preset)
    {
        if (preset.SearchFilter.SearchEnabled)
        {
            Log.Warning("Search filter is not implemented");
        }
        
        if (preset.ThumbnailTag.Enable.Enable)
        {
            Log.Warning("Thumbnail Tagging is not supported");
        }
        
        if (preset.SongFilter.DownloadCount.Enable)
        {
            Log.Warning("Download count is not a thing anymore");
        }
        
        if (preset.SongFilter.PlayCount.Enable)
        {
            Log.Warning("Play count is not supported");
        }

        if (preset.SongFilter.GeneratedSong.Enable || preset.SongFilter.SageScore.Enable)
        {
            Log.Warning("AI maps are not supported");
        }
        
        if (preset.SongFilter.FilterChinese.Enable)
        {
            Log.Warning("Chinese filter is not implemented");
        }

        if (preset.SongFilter.MapSeconds.Enable)
        {
            Log.Warning("Map seconds is not supported");
        }
        
        if (preset.SongFilter.MapLength.Enable)
        {
            Log.Warning("Map length is not supported");
        }
        
        if (preset.SongFilter.Offset.Enable)
        {
            Log.Warning("Offset is not supported");
        }
        
        if (preset.SongFilter.Events.Enable)
        {
            Log.Warning("Events is not supported");
        }
        
        if (preset.SongFilter.ParityErrors.Enable || preset.SongFilter.ParityWarns.Enable || preset.SongFilter.ParityResets.Enable)
        {
            Log.Warning("Parity data is not supported");
        }
        
        if (preset.SongFilter.MaxScore.Enable)
        {
            Log.Warning("Max score is not supported");
        }
    }

    private static void CombineRange<T>(FilterOptions.RangeOption<T> o, LegacyPreset.IMinMaxSetting<T> s) where T : struct, IComparable<T>
    {
        var min = s.Min;
        var max = s.Max;
        var min1 = o.Min;
        var max1 = o.Min;
        var min2 = s.Min;
        var max2 = s.Max;
        if (o.Enable)
        {
            min = min1.HasValue && min2.HasValue ? min1.Value.CompareTo(min2.Value) > 0 ? min1 : min2 : min1 ?? min2;
            max = max1.HasValue && max2.HasValue ? max1.Value.CompareTo(max2.Value) < 0 ? max1 : max2 : max1 ?? max2;
        }

        o.Enable = true;
        o.Min = min;
        o.Max = max;
    }

    private static void MergeBeatSaverSetting(FilterOptions options, LegacyPreset.BeatSaverSetting setting)
    {
        Log.Debug("Merging BeatSaver source settings into filter options");
        if (!string.IsNullOrWhiteSpace(setting.SearchKeyword) || setting.StartPage.HasValue)
        {
            Log.Warning("BeatSaver search and page number are not supported!");
        }

        if (setting.GeneratedSong.Enable)
        {
            Log.Warning("AI generated songs are not supported!");
        }

        if (setting.RankedSong.Enable && setting.RankedSong.IsRanked)
        {
            if (options.ScoreSaberRanking.Enable)
            {
                Log.Warning("Overwriting ScoreSaber option to {Status}", RankingStatus.Ranked);
            }

            options.ScoreSaberRanking.Enable = true;
            options.ScoreSaberRanking.Filter = new HashSet<RankingStatus> { RankingStatus.Ranked };
        }

        if (setting.Difficulty.Enable)
        {
            options.FullSpread.Enable = options.FullSpread.Enable || setting.Difficulty.IsFullSpread;
        }

        if (setting.Bpm.Enable)
        {
            Log.Debug("Merging BeatSaver BPM setting into filter options");
            CombineRange(options.Bpm, setting.Bpm);
        }

        if (setting.Nps.Enable)
        {
            Log.Debug("Merging BeatSaver NPS setting into filter options");
            CombineRange(options.Nps, setting.Nps);
        }

        if (setting.Duration.Enable)
        {
            Log.Debug("Merging BeatSaver duration setting into filter options");
            CombineRange(options.Duration, setting.Duration);
        }

        if (setting.UploadTime.Enable)
        {
            Log.Debug("Merging BeatSaver upload time setting into filter options");
            CombineRange(options.UploadTime, setting.UploadTime);
        }

        if (setting.Rating.Enable)
        {
            Log.Debug("Merging BeatSaver rating setting into filter options");
            CombineRange(options.Rating, setting.Rating);
        }

        if (setting.RequireMods.Enable)
        {
            options.RequireMods.Enable = true;
            options.RequireMods.Filter = options.RequireMods.Filter
                .Concat(setting.RequireMods.RequireMods.ToMMods()).ToHashSet();
        }

        if (setting.ExcludeMods.Enable)
        {
            options.ExcludeMods.Enable = true;
            options.ExcludeMods.Filter = options.ExcludeMods.Filter
                .Concat(setting.ExcludeMods.ExcludeMods.ToMMods()).Distinct().ToList();
        }
    }

    private static void MergeScoreSaverSetting(FilterOptions options, LegacyPreset.ScoreSaberSetting setting)
    {
        Log.Debug("Merging ScoreSaber source setting into filter options");
        options.ScoreSaberRanking.Enable = true;
        options.ScoreSaberRanking.Filter = new HashSet<RankingStatus> { RankingStatus.Ranked };
        CombineRange(options.ScoreSaberStars, setting.StarDifficulty);
    }

    private static void MergeMapperSetting(FilterOptions options, LegacyPreset.MapperSetting setting)
    {
        var url = setting.MapperAddress;
        Log.Debug("Extracting uploader id from mapper url: {Url}", url);
        var match = MapperUrlRegex.Match(url);
        if (match.Success && match.Groups.Count == 2 && int.TryParse(match.Groups[1].Value, out var id))
        {
            if (options.UploaderId.Enable && options.UploaderId.Filter.HasValue)
            {
                Log.Warning("Overwriting existing UploaderID filter to {New}", id);
            }

            options.UploaderId.Enable = true;
            options.UploaderId.Filter = id;
        }
        else
        {
            Log.Warning("Failed to find uploader id from mapper url: {Url}", url);
        }
    }

    private static FilterOptions ConvertFilterOptions(LegacyPreset.SongFilterSetting setting)
    {
        return new FilterOptions
        {
            UploaderId = new(int.TryParse(setting.UploaderId.Content, out var id) ? id : null)
            {
                Enable = setting.UploaderId.Enable
            },
            UploaderName = new(setting.UploaderName.Content)
            {
                Enable = setting.UploaderName.Enable
            },
            UploadTime = new()
            {
                Enable = setting.UploadTime.Enable,
                Min = setting.UploadTime.Min,
                Max = setting.UploadTime.Max
            },
            IncludeTags = new()
            {
                Enable = setting.Tags.Include.Enable,
                Filter = setting.Tags.Include.Content.ToHashSet(),
                IsOr = !setting.Tags.Include.And
            },
            ExcludeTags = new(setting.Tags.Exclude.Content.ToList())
            {
                Enable = setting.Tags.Exclude.Enable
            },
            UpVotes = new()
            {
                Enable = setting.UpVotes.Enable,
                Min = setting.UpVotes.Min,
                Max = setting.UpVotes.Max
            },
            UpVotePercentage = new()
            {
                Enable = setting.UpVotePercentage.Enable,
                Min = setting.UpVotePercentage.Min,
                Max = setting.UpVotePercentage.Max
            },
            DownVotes = new()
            {
                Enable = setting.DownVotes.Enable,
                Min = setting.DownVotes.Min,
                Max = setting.DownVotes.Max
            },
            DownVotePercentage = new()
            {
                Enable = setting.DownVotePercentage.Enable,
                Min = setting.DownVotePercentage.Min,
                Max = setting.DownVotePercentage.Max
            },
            Rating = new()
            {
                Enable = setting.Rating.Enable,
                Min = setting.Rating.Min,
                Max = setting.Rating.Max
            },
            IncludeCharacteristics = new()
            {
                Enable = setting.IncludeCharacteristics.Enable,
                Filter = setting.IncludeCharacteristics.Characteristics.Select(LegacyExtensions.ToMCharacteristic)
                    .ToHashSet(),
                IsOr = true
            },
            IncludeDifficulties = new()
            {
                Enable = setting.IncludeDifficulties.Enable,
                Filter = setting.IncludeDifficulties.Difficulties.Select(LegacyExtensions.ToMDifficulty).ToHashSet(),
                IsOr = !setting.IncludeDifficulties.And
            },
            RequireMods = new()
            {
                Enable = setting.RequireMods.Enable,
                Filter = setting.RequireMods.Mods.ToMMods().ToHashSet(),
                IsOr = true
            },
            ExcludeMods = new(setting.ExcludeMods.Mods.ToMMods())
            {
                Enable = setting.ExcludeMods.Enable
            },
            Bpm = new()
            {
                Enable = setting.Bpm.Enable,
                Min = setting.Bpm.Min,
                Max = setting.Bpm.Max
            },
            Duration = new()
            {
                Enable = setting.Duration.Enable,
                Min = setting.Duration.Min,
                Max = setting.Duration.Max
            },
            Njs = new()
            {
                Enable = setting.Njs.Enable,
                Min = setting.Njs.Min,
                Max = setting.Njs.Max
            },
            Nps = new()
            {
                Enable = setting.Nps.Enable,
                Min = setting.Nps.Min,
                Max = setting.Nps.Max
            },
            Notes = new()
            {
                Enable = setting.Notes.Enable,
                Min = setting.Notes.Min,
                Max = setting.Notes.Max
            },
            Bombs = new()
            {
                Enable = setting.Bombs.Enable,
                Min = setting.Bombs.Min,
                Max = setting.Bombs.Max
            },
            Walls = new()
            {
                Enable = setting.Walls.Enable,
                Min = setting.Walls.Min,
                Max = setting.Walls.Max
            },
            ScoreSaberRanking = new(setting.RankedSong.IsRanked
                ? new HashSet<RankingStatus> { RankingStatus.Ranked }
                : new HashSet<RankingStatus> { RankingStatus.Unranked, RankingStatus.Qualified })
            {
                Enable = setting.RankedSong.Enable
            },
            ScoreSaberStars = new()
            {
                Enable = setting.Stars.Enable,
                Min = setting.Stars.Min,
                Max = setting.Stars.Max
            },
            Chinese = new()
            {
                Enable = setting.FilterChinese.Enable
            }
        };
    }
}