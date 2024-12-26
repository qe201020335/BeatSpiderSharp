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
            .MinimumLevel.Debug()
            .WriteTo.File("BeatSpiderCLI.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console();
    }

    public async Task Run(string[] args)
    {
        Log.Information("BeatSpiderCLI!");
        
        if (args.Length == 0)
        {
            Log.Error("No arguments provided");
            return;
        }

        await InitAsync();

        var preset = LoadLegacyPreset(args[0]);

        if (preset == null)
        {
            Log.Warning("Failed to load preset");
            return;
        }

        Log.Information("Song source: {Source}", preset.SongSource);

        var songSource = new SongDetailsSongs(SongDetails) { ReverseOrder = true };
        var presetFilter = new LegacyFilter(preset) { LogExclusions = true };

        var allSongs = songSource.GetSongs();
        var filteredSongs = presetFilter.Filter(allSongs);

        Log.Information("Filtered songs: {Count}", filteredSongs.Count());

        // WriteLegacyPreset(preset, "./output.json");
    }
}