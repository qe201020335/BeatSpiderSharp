using BeatSpiderSharp.Core.Filters;
using BeatSpiderSharp.Core.Interfaces;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.Models.Preset;
using BeatSpiderSharp.Core.Models.Preset.Enums;
using BeatSpiderSharp.Core.SongSource;
using Serilog;
using SongDetailsCache;

namespace BeatSpiderSharp.Core;

public abstract class BeatSpider
{
    protected SpecialFolders SpecialFolders { get; } = new();

    protected SongDetails SongDetails { get; private set; } = null!;
    
    protected bool Verbose { get; set; }

    protected BeatSpider()
    {
        SetupLogging();
        SongDetails.SetCacheDirectory(SpecialFolders.DataFolder);
    }

    private void SetupLogging()
    {
        var configuration = new LoggerConfiguration();
        ConfigureLogger(configuration);
        Log.Logger = configuration.CreateLogger();
    }

    protected virtual void ConfigureLogger(LoggerConfiguration configuration)
    {
    }
    
    protected virtual async Task InitAsync()
    {
        Log.Information("BeatSpider initializing");
        SongDetails = await SongDetails.Init();
        Log.Debug("SongDetails initialized");
    }

    protected ISongSource GetSongSource(InputConfig input)
    {
        Log.Information("Song source: {Source}", input.Source);

        return input.Source switch
        {
            SongInputSource.Playlists => new PlaylistSongs(input.Playlists, SongDetails, SpecialFolders.TempFolder),
            SongInputSource.ManualInput => new ManualSongInput(input.ManualInput, SongDetails),
            _ => new SongDetailsSongs(SongDetails) { ReverseOrder = true }
        };
    }

    protected async Task<IEnumerable<BeatSpiderSong>?> FilterSongsAsync(IEnumerable<BeatSpiderSong> songs, Preset preset)
    {
        var detailFilterOptions = preset.DetailFilters;
        if (detailFilterOptions.Count == 0)
        {
            Log.Warning("No filters specified");
            return songs;
        }

        var detailFilters = detailFilterOptions
            .Select(options => new DetailFilter(options) { LogExclusions = Verbose })
            .ToList();
        try
        {
            await Task.WhenAll(detailFilters.Select(filter => filter.InitAsync()));
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to initialize one or more filters");
            return null;
        }

        return songs.Where(song => detailFilters.Any(filter => filter.FilterSong(song)));
    }
    
    protected int OutputSongs(IEnumerable<BeatSpiderSong> songs, OutputConfig output)
    {
        if (output.LimitSongs && output.MaxSongs.HasValue && output.MaxSongs.Value > 0)
        {
            Log.Information("Applying count limit: {Count}", output.MaxSongs.Value);
            songs = songs.Take(output.MaxSongs.Value);
        }

        var count = 0;
        if (Verbose)
        {
            foreach (var song in songs)
            {
                Log.Debug("Song {Bsr} included", song.Bsr);
                count++;
            }
        }
        else
        {
            count = songs.Count();
        }
        
        // TODO
        // if (output.SavePlaylist)
        // {
        //     Log.Information("Saving playlist to {Path}", output.PlaylistPath);
        //     // TODO
        // }
        //
        // if (output.DownloadSongs)
        // {
        //     Log.Information("Downloading songs to {Path}", output.DownloadPath);
        //     // TODO
        // }
        
        return count;
    }
}