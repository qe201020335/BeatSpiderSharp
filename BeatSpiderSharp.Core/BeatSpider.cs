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
    protected SongDetails SongDetails { get; private set; } = null!;
    
    protected bool Verbose { get; set; }

    protected BeatSpider()
    {
        SetupLogging();
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
            SongInputSource.Playlists => new PlaylistSongs(input.Playlists, SongDetails),
            SongInputSource.ManualInput => new ManualSongInput(input.ManualInput, SongDetails),
            _ => new SongDetailsSongs(SongDetails) { ReverseOrder = true }
        };
    }

    protected IEnumerable<BeatSpiderSong> FilterSongs(IEnumerable<BeatSpiderSong> songs, Preset preset)
    {
        // TODO
        return [];
    }
}