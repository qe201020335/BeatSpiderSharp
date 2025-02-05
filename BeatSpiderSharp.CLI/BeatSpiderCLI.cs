using BeatSpiderSharp.CLI.Command;
using BeatSpiderSharp.Core;
using BeatSpiderSharp.Core.Filters;
using BeatSpiderSharp.Core.Interfaces;
using BeatSpiderSharp.Core.SongSource;
using Serilog;
using SongDetailsCache;

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

    public async Task Run(Options options)
    {
        Log.Information("BeatSpiderCLI!");
        Log.Debug("Options: {@Options}", options);

        await InitAsync();

        var preset = LoadLegacyPreset(options.InputPreset);

        if (preset == null)
        {
            Log.Warning("Failed to load preset");
            return;
        }

        Log.Information("Song source: {Source}", preset.SongSource);

        var songSource = new SongDetailsSongs(SongDetails) { ReverseOrder = true };
        var presetFilter = new LegacyFilter(preset) { LogExclusions = false, LogInclusions = options.Verbose };

        var allSongs = songSource.GetSongs();
        var filteredSongs = presetFilter.Filter(allSongs);

        Log.Information("Filtered songs: {Count}", filteredSongs.Count());

        // WriteLegacyPreset(preset, "./output.json");
    }
}