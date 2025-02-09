using BeatSaverSharp;
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

    protected BeatSaver BeatSaver { get; } = new("BeatSpiderSharp", new Version(1, 0, 0));

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
            await Task.WhenAll(detailFilters.Select(filter => filter.InitAsync(BeatSaver)));
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to initialize one or more filters");
            return null;
        }

        return songs.Where(song => detailFilters.Any(filter => filter.FilterSong(song)));
    }

    protected int OutputSongs(IEnumerable<BeatSpiderSong> songs, Preset preset)
    {
        var output = preset.Output;
        if (output.LimitSongs && output.MaxSongs.HasValue && output.MaxSongs.Value > 0)
        {
            Log.Information("Applying count limit: {Count}", output.MaxSongs.Value);
            songs = songs.Take(output.MaxSongs.Value);
        }

        if (Verbose)
        {
            songs = songs.Select(song =>
            {
                Log.Information("Song {Bsr} ({Title} - {Mapper}) included", song.Bsr, song.SongDetails.songName, song.SongDetails.uploaderName);
                return song;
            });
        }

        var consolidated = songs.ToArray();

        // Process name variables
        var name = preset.Name.Replace("[日期]", DateTime.Today.ToString("yyyy-MM-dd"));
        var songPath = output.DownloadPath.Replace("[日期]", DateTime.Today.ToString("yyyy-MM-dd"));

        // TODO
        // if (output.DownloadSongs)
        // {
        //     Log.Information("Querying BeatSaver for map download info");
        //     
        //     Log.Information("Downloading songs to {Path}", output.DownloadPath);
        //     Parallel.
        // }

        if (output.SavePlaylist)
        {
            Log.Information("Saving playlist to {Path}", output.PlaylistPath);
            var exporter = new PlaylistExporter { PostProcess = output.PostProcessPlaylist };
            exporter.Export(consolidated, name, preset.Author, preset.Description, output.PlaylistPath);
        }

        return consolidated.Length;
    }
}