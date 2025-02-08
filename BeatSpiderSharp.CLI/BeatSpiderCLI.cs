using BeatSpiderSharp.CLI.Command;
using BeatSpiderSharp.Core;
using BeatSpiderSharp.Core.Legacy;
using BeatSpiderSharp.Core.Models.Preset;
using BeatSpiderSharp.Core.Utilities;
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
            // .WriteTo.File("BeatSpiderCLI.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console();
    }

    public async Task<int> Run(Options options)
    {
        Log.Information("BeatSpiderCLI!");

#if DEBUG
        Log.Debug("Options: {@Options}", options);
#endif

        Verbose = options.Verbose;

        await InitAsync();

        Preset preset;

        if (options.InputIsLegacy)
        {
            var legacy = LegacyPresetLoader.LoadLegacyPreset(options.InputPreset);

            if (legacy == null)
            {
                Log.Warning("Failed to load preset");
                return -1;
            }

            // LegacyPresetLoader.SaveLegacyPreset(preset, "./output.json");
            preset = legacy.ConvertToPreset(Path.GetFileNameWithoutExtension(options.InputPreset));

            if (!string.IsNullOrEmpty(options.SaveConvertedPresetPath))
            {
                Log.Information("Saving converted preset");
                PresetLoader.SavePreset(preset, options.SaveConvertedPresetPath);
            }

            if (options.ConvertPresetAndExit)
            {
                if (!string.IsNullOrEmpty(options.SaveConvertedPresetPath))
                {
                    Log.Information("[ConvertPresetAndExit] Preset has been converted, exiting");
                }
                else
                {
                    Log.Warning("[ConvertPresetAndExit] No path given for converted preset to save, still exiting");
                }

                return 0;
            }
        }
        else
        {
            var p = PresetLoader.LoadPreset(options.InputPreset);

            if (p == null)
            {
                Log.Warning("Failed to load preset");
                return -1;
            }

            preset = p;
            if (!string.IsNullOrEmpty(options.SaveConvertedPresetPath))
            {
                Log.Warning("Cannot save converted preset when input is not a legacy preset");
            }

            if (options.ConvertPresetAndExit)
            {
                Log.Warning("[ConvertPresetAndExit] No change has been made to the preset, exiting");
                return 1;
            }
        }

        var songSource = GetSongSource(preset.Input);
        var allSongs = songSource.GetSongs();
        var filteredSongs = FilterSongs(allSongs, preset);

        Log.Information("Filtered songs: {Count}", filteredSongs.Count());

        return 0;
    }
}