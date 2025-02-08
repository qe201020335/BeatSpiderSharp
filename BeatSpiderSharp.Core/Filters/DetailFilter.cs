using BeatSpiderSharp.Core.Interfaces;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.Models.Preset;
using BeatSpiderSharp.Core.Models.Preset.Enums;
using BeatSpiderSharp.Core.Utilities.Extensions;
using Serilog;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.Filters;

public class DetailFilter: ISongFilter
{
    private readonly FilterOptions _options;

    public bool LogExclusions { get; init; }
    
    public DetailFilter(FilterOptions options)
    {
        _options = options;
    }

    public async Task InitAsync()
    {
        //TODO Deal with MapperID
    }

    public bool FilterSong(BeatSpiderSong song)
    {
        var filter = _options;
        var details = song.SongDetails;
        
        // TODO SongDetails doesn't have uploader id
        
        if (filter.UploaderName && filter.UploaderName.Filter != details.uploaderName)
        {
            LogExclusion(song, "Uploader name doesn't match");
            return false;
        }
        
        if (filter.UploadTime && !filter.UploadTime.InRange(DateTimeOffset.FromUnixTimeSeconds(details.uploadTimeUnix)))
        {
            LogExclusion(song, "Upload time not in range");
            return false;
        }
        
        if (filter.IncludeTags)
        {
            var required = filter.IncludeTags.Filter;
            var pass = filter.IncludeTags.IsOr ? required.Any(details.HasTag) : required.All(details.HasTag);
            if (!pass)
            {
                LogExclusion(song, "Required tags not found");
                return false;
            }
        }
        
        if (filter.ExcludeTags && filter.ExcludeTags.Filter.Any(details.HasTag))
        {
            LogExclusion(song, "Excluded tags found");
            return false;
        }
        
        if (filter.UpVotes && !filter.UpVotes.InRange((int) details.upvotes))
        {
            LogExclusion(song, "Up votes not in range");
            return false;
        }
        
        if (filter.UpVotePercentage && !filter.UpVotePercentage.InRange(details.upvotes / (float) (details.upvotes + details.downvotes) * 100))
        {
            LogExclusion(song, "Up vote percentage not in range");
            return false;
        }
        
        if (filter.DownVotes && !filter.DownVotes.InRange((int) details.downvotes))
        {
            LogExclusion(song, "Down votes not in range");
            return false;
        }
        
        if (filter.DownVotePercentage && !filter.DownVotePercentage.InRange(details.downvotes / (float) (details.upvotes + details.downvotes) * 100))
        {
            LogExclusion(song, "Down vote percentage not in range");
            return false;
        }
        
        if (filter.Rating && !filter.Rating.InRange(details.rating * 100))
        {
            LogExclusion(song, "Rating not in range");
            return false;
        }

        if (filter.FullSpread)
        {
            var pass = Enum.GetValues<MapCharacteristic>().Where(chara => chara != MapCharacteristic.Custom)
                .Any(chara => Enum.GetValues<MapDifficulty>().All(diff => details.GetDifficulty(out _, diff, chara)));
            if (!pass)
            {
                LogExclusion(song, "Not full spread");
                return false;
            }
        }
        
        if (filter.IncludeCharacteristics)
        {
            var mapCharas = details.difficulties.Select(diff => diff.characteristic.ToMCharacteristic()).ToHashSet();
            if (!filter.IncludeCharacteristics.SatisfiedBy(mapCharas))
            {
                LogExclusion(song, "Required characteristics not found");
                return false;
            }
        }
        
        if (filter.IncludeDifficulties)
        {
            var mapDiffs = details.difficulties.Select(diff => diff.difficulty.ToMDifficulty()).ToHashSet();
            if (!filter.IncludeDifficulties.SatisfiedBy(mapDiffs))
            {
                LogExclusion(song, "Required difficulties not found");
                return false;
            }
        }

        if (filter.RequireMods)
        {
            var pass = details.difficulties.Any(diff => filter.RequireMods.SatisfiedBy(diff.mods.ToMMods()));
            if (!pass)
            {
                LogExclusion(song, "Required mods not found");
                return false;
            }
        }
        
        if (filter.ExcludeMods)
        {
            var excluded = filter.ExcludeMods.Filter;
            // excluded.Any(diff.mods.ToMMods().Contains) --> diff contains any of the excluded mods
            if (details.difficulties.All(diff => excluded.Any(diff.mods.ToMMods().Contains)))
            {
                LogExclusion(song, "All difficulties contain excluded mods");
                return false;
            }
        }

        if (filter.Bpm && !filter.Bpm.InRange(details.bpm))
        {
            LogExclusion(song, "BPM not in range");
            return false;
        }
        
        if (filter.Duration && !filter.Duration.InRange((int) details.songDurationSeconds))
        {
            LogExclusion(song, "Duration not in range");
            return false;
        }
        
        if (filter.Njs && !details.difficulties.Any(difficulty => filter.Njs.InRange(difficulty.njs)))
        {
            LogExclusion(song, "NJS not in range");
            return false;
        }
        
        if (filter.Nps && !details.difficulties.Any(difficulty => filter.Nps.InRange(difficulty.notes / (float) details.songDurationSeconds)))
        {
            LogExclusion(song, "NPS not in range");
            return false;
        }
                
        if (filter.Notes && !details.difficulties.Any(difficulty => filter.Njs.InRange(difficulty.notes)))
        {
            LogExclusion(song, "Notes not in range");
            return false;
        }
        
        if (filter.Bombs && !details.difficulties.Any(difficulty => filter.Bombs.InRange((int) difficulty.bombs)))
        {
            LogExclusion(song, "Bombs not in range");
            return false;
        }
        
        if (filter.Walls && !details.difficulties.Any(difficulty => filter.Walls.InRange((int) difficulty.obstacles)))
        {
            LogExclusion(song, "Walls not in range");
            return false;
        }

        if (filter.ScoreSaberRanking)
        {
            var states = details.rankedStates;
            var pass = filter.ScoreSaberRanking.Filter.Any(status => status switch 
            {
                RankingStatus.Unranked => !states.HasFlag(RankedStates.ScoresaberRanked) && !states.HasFlag(RankedStates.ScoresaberQualified),
                RankingStatus.Ranked => states.HasFlag(RankedStates.ScoresaberRanked),
                RankingStatus.Qualified => states.HasFlag(RankedStates.ScoresaberQualified),
                _ => false
            });
            
            if (!pass)
            {
                LogExclusion(song, "Required ScoreSaber ranking status not found");
                return false;
            }
        }

        if (filter.BeatLeaderRanking)
        {
            var states = details.rankedStates;
            var pass = filter.BeatLeaderRanking.Filter.Any(status => status switch 
            {
                RankingStatus.Unranked => !states.HasFlag(RankedStates.BeatleaderRanked) && !states.HasFlag(RankedStates.BeatleaderQualified),
                RankingStatus.Ranked => states.HasFlag(RankedStates.BeatleaderRanked),
                RankingStatus.Qualified => states.HasFlag(RankedStates.BeatleaderQualified),
                _ => false
            });
            
            if (!pass)
            {
                LogExclusion(song, "Required BeatLeader ranking status not found");
                return false;
            }
        }

        if (filter.ScoreSaberStars)
        {
            var pass = details.difficulties.Any(diff => filter.ScoreSaberStars.InRange(diff.stars));
            if (!pass)
            {
                LogExclusion(song, "ScoreSaber stars not in range");
                return false;
            }
        }
        
        if (filter.BeatLeaderStars)
        {
            var pass = details.difficulties.Any(diff => filter.BeatLeaderStars.InRange(diff.starsBeatleader));
            if (!pass)
            {
                LogExclusion(song, "BeatLeader stars not in range");
                return false;
            }
        }

        // TODO Implement chinese filter
        // if (filter.FilterChinese)
        // {
        //     // ??
        // }
        return true;
    }
    
    private void LogExclusion(BeatSpiderSong song, string reason)
    {
        if (LogExclusions) Log.Debug("Song {Bsr} excluded: {Reason}", song.Bsr, reason);
    }
}