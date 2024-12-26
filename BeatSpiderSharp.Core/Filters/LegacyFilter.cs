using BeatSpiderSharp.Core.Interfaces;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.SongSource;
using BeatSpiderSharp.Core.Utilities.Extensions;
using Serilog;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.Filters;

public class LegacyFilter : ISongFilter
{
    private readonly LegacyPreset _preset;

    private readonly LegacyPreset.SongFilterSetting _detailFilter;
    public bool LogExclusions { get; set; } = false;
    
    public bool LogInclusions { get; set; } = false;
    
    public LegacyFilter(LegacyPreset preset)
    {
        _preset = preset;
        _detailFilter = preset.SongFilter;
        
        // TODO Do sanity check
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
            Log.Warning("Play count is not implemented");
        }

        if (preset.SongFilter.GeneratedSong.Enable || preset.SongFilter.SageScore.Enable)
        {
            Log.Warning("AI maps are not included in the data");
        }
        
        if (preset.SongFilter.FilterChinese.Enable)
        {
            Log.Warning("Chinese filter is not implemented");
        }

        if (preset.SongFilter.MapSeconds.Enable)
        {
            Log.Warning("Map seconds is not implemented");
        }
        
        if (preset.SongFilter.MapLength.Enable)
        {
            Log.Warning("Map length filter may not be accurate");
        }
        
        if (preset.SongFilter.Offset.Enable)
        {
            Log.Warning("Offset is not implemented");
        }
        
        if (preset.SongFilter.Events.Enable)
        {
            Log.Warning("Events is not implemented");
        }
        
        if (preset.SongFilter.ParityErrors.Enable || preset.SongFilter.ParityWarns.Enable || preset.SongFilter.ParityResets.Enable)
        {
            Log.Warning("Parity data is not implemented");
        }
        
        if (preset.SongFilter.MaxScore.Enable)
        {
            Log.Warning("Max score is not implemented");
        }
    }
    
    public IEnumerable<BeatSpiderSong> Filter(IEnumerable<BeatSpiderSong> songs)
    {
        var filtered = songs.Where(FilterSong);
        
        var limit = _preset.Limits.Count;
        if (!limit.Enable || limit.Content == null)
        {
            return filtered;
        }
        
        var count = limit.Content.Value;
        if (count > 0)
        {
            Log.Debug("Applying count limit: {Count}", count);
            return filtered.Take(count);
        }

        Log.Error("Count limit is not positive: {Count}", count);
        return [];
    }
    
    private bool FilterSong(BeatSpiderSong song)
    {
        //TODO implement search filter
        var filter = _detailFilter;
        var details = song.SongDetails;
        
        // TODO SongDetails doesn't have uploader id
        
        if (filter.UploaderName.Enable && filter.UploaderName.Content != details.uploaderName)
        {
            LogExclusion(song, "Uploader name doesn't match");
            return false;
        }

        if (filter.RequireMods.Enable)
        {
            // TODO optimize
            var required = filter.RequireMods.Mods.ToMapMods();
            var hasMods = details.difficulties.Any(diff => diff.mods.HasFlag(required));
            
            if (!hasMods)
            {
                LogExclusion(song, "Required mods not found");
                return false;
            }
        }
        
        if (filter.ExcludeMods.Enable)
        {
            // TODO optimize
            var excluded = filter.ExcludeMods.Mods.ToMapMods();
            var hasMods = details.difficulties.Any(diff => (diff.mods & excluded) != 0);
            
            if (hasMods)
            {
                LogExclusion(song, "Excluded mods found");
                return false;
            }
        }

        if (filter.IncludeCharacteristics.Enable)
        {
            // TODO optimize
            var characteristics = filter.IncludeCharacteristics.Characteristics.Select(c => c.ToMapCharacteristic()).ToHashSet();
            var hasCharacteristics = details.difficulties.Any(diff => characteristics.Contains(diff.characteristic));
            
            if (!hasCharacteristics)
            {
                LogExclusion(song, "Required characteristics not found");
                return false;
            }
        }

        if (filter.IncludeDifficulties.Enable)
        {
            // TODO optimize
            var difficulties = filter.IncludeDifficulties.Difficulties.Select(c => c.ToMapDifficulty()).ToHashSet();

            if (filter.IncludeDifficulties.And)
            {
                var mapDiffs = details.difficulties.Select(diff => diff.difficulty).ToHashSet();
                
                mapDiffs.IntersectWith(difficulties);
                
                if (mapDiffs.Count != difficulties.Count)
                {
                    LogExclusion(song, "Required difficulties not found");
                    return false;
                }
            }
            else
            {
                // or
                var hasDifficulties = details.difficulties.Any(diff => difficulties.Contains(diff.difficulty));
            
                if (!hasDifficulties)
                {
                    LogExclusion(song, "Required difficulties not found");
                    return false;
                }
            }
        }

        // Download count is not being recorded anymore
        
        // TODO SongDetails doesn't have play count

        
        if (filter.UpVotes.Enable && !filter.UpVotes.InRange((int) details.upvotes))
        {
            LogExclusion(song, "Up votes not in range");
            return false;
        }
        
        if (filter.UpVotePercentage.Enable && !filter.UpVotePercentage.InRange(details.upvotes / (float) (details.upvotes + details.downvotes) * 100))
        {
            LogExclusion(song, "Up vote percentage not in range");
            return false;
        }
        
        if (filter.DownVotes.Enable && !filter.DownVotes.InRange((int) details.downvotes))
        {
            LogExclusion(song, "Down votes not in range");
            return false;
        }
        
        if (filter.DownVotePercentage.Enable && !filter.DownVotePercentage.InRange(details.downvotes / (float) (details.upvotes + details.downvotes) * 100))
        {
            LogExclusion(song, "Down vote percentage not in range");
            return false;
        }
        
        if (filter.Rating.Enable && !filter.Rating.InRange(details.rating * 100))
        {
            LogExclusion(song, "Rating not in range");
            return false;
        }
        
        // SongDetails only has non-ai songs 

        if (filter.RankedSong.Enable)
        {
            var ssRanked = details.rankedStates.HasFlag(RankedStates.ScoresaberRanked);
            if (ssRanked != filter.RankedSong.IsRanked)
            {
                LogExclusion(song, "ScoreSaber Ranked status doesn't match");
                return false;
            }
        }

        // TODO Implement chinese filter
        // if (filter.FilterChinese.Enable)
        // {
        //     // ??
        // }
        
        if (filter.Bpm.Enable && !filter.Bpm.InRange(details.bpm))
        {
            LogExclusion(song, "BPM not in range");
            return false;
        }
        
        if (filter.Duration.Enable && !filter.Duration.InRange((int) details.songDurationSeconds))
        {
            LogExclusion(song, "Duration not in range");
            return false;
        }

        // TODO SongDetails doesn't have map seconds

        // simple conversion, may not be very accurate
        if (filter.MapLength.Enable && !filter.MapLength.InRange(details.songDurationSeconds * details.bpm / 60))
        {
            LogExclusion(song, "Beats not in range");
            return false;
        }
        
        if (filter.Njs.Enable && !details.difficulties.Any(difficulty => filter.Njs.InRange(difficulty.njs)))
        {
            LogExclusion(song, "NJS not in range");
            return false;
        }
        
        // TODO SongDetails doesn't have offset
        
        if (filter.Notes.Enable && !details.difficulties.Any(difficulty => filter.Njs.InRange(difficulty.notes)))
        {
            LogExclusion(song, "Notes not in range");
            return false;
        }
        
        if (filter.Nps.Enable && !details.difficulties.Any(difficulty => filter.Nps.InRange(difficulty.notes / (float) details.songDurationSeconds)))
        {
            LogExclusion(song, "NPS not in range");
            return false;
        }
        
        if (filter.Bombs.Enable && !details.difficulties.Any(difficulty => filter.Bombs.InRange((int) difficulty.bombs)))
        {
            LogExclusion(song, "Bombs not in range");
            return false;
        }
        
        // TODO SongDetails doesn't have events
        
        if (filter.Walls.Enable && !details.difficulties.Any(difficulty => filter.Walls.InRange((int) difficulty.obstacles)))
        {
            LogExclusion(song, "Walls not in range");
            return false;
        }
        
        if (filter.Stars.Enable && !details.difficulties.Any(difficulty => filter.Stars.InRange(difficulty.stars)))
        {
            LogExclusion(song, "ScoreSaber stars not in range");
            return false;
        }
        
        // TODO SongDetails doesn't have parity data

        if (filter.UploadTime.Enable && !filter.UploadTime.InRange(DateTimeOffset.FromUnixTimeSeconds(details.uploadTimeUnix)))
        {
            LogExclusion(song, "Upload time not in range");
            return false;
        }

        if (filter.Tags.Include.Enable)
        {
            var tags = filter.Tags.Include;
            var pass = tags.And ? tags.Content.All(tag => details.HasTag(tag)) : tags.Content.Any(tag => details.HasTag(tag));
            
            if (!pass)
            {
                LogExclusion(song, "Required tags not found");
                return false;
            }
        }
        
        if (filter.Tags.Exclude.Enable)
        {
            var tags = filter.Tags.Exclude;
            var pass = tags.And ? tags.Content.All(tag => !details.HasTag(tag)) : tags.Content.Any(tag => !details.HasTag(tag));
            
            if (!pass)
            {
                LogExclusion(song, "Excluded tags found");
                return false;
            }
        }
        
        // TODO SongDetails doesn't have ai generated song data so no SageScore
        
        // TODO SongDetails doesn't have MaxScore
        
        if (LogInclusions) Log.Debug("Song {Bsr} included", song.Bsr);
        return true;
    }
    
    private void LogExclusion(BeatSpiderSong song, string reason)
    {
        if (LogExclusions) Log.Debug("Song {Bsr} excluded: {Reason}", song.Bsr, reason);
    }
}