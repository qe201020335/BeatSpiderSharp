using BeatSpiderSharp.CLI.Command;
using BeatSpiderSharp.Core;
using BeatSpiderSharp.Core.Legacy;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.SongSource;
using Serilog;

namespace BeatSpiderSharp.CLI;

public class BeatSpiderCLI : BeatSpider
{
    protected override void ConfigureLogger(LoggerConfiguration configuration)
    {
        base.ConfigureLogger(configuration);
        configuration
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .WriteTo.File("BeatSpiderCLI.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console();
    }

    public async Task<int> Run(Options options)
    {
        Log.Information("BeatSpiderCLI!");
        Log.Debug("Options: {@Options}", options);

        await InitAsync();

        var preset = LegacyPresetLoader.LoadLegacyPreset(options.InputPreset);

        if (preset == null)
        {
            Log.Warning("Failed to load preset");
            return 1;
        }

        Log.Information("Song source: {Source}", preset.SongSource);

        var songSource = preset.SongSource switch
        {
            LegacyPreset.DataSource.Playlist => new PlaylistSongs(preset.PlaylistInput.Path, SongDetails),
            LegacyPreset.DataSource.ManualInput => new ManualSongInput(preset.ManualSongInput.Songs, SongDetails),
            _ => new SongDetailsSongs(SongDetails) { ReverseOrder = true }
        };
        var presetFilter = new LegacyFilter(preset) { LogExclusions = false, LogInclusions = options.Verbose };

        var allSongs = songSource.GetSongs();
        var filteredSongs = presetFilter.Filter(allSongs);

        Log.Information("Filtered songs: {Count}", filteredSongs.Count());

        // LegacyPresetLoader.SaveLegacyPreset(preset, "./output.json");
        return 0;
    }
}