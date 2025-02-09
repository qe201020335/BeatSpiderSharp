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

            if (!string.IsNullOrWhiteSpace(options.SaveConvertedPresetPath))
            {
                Log.Information("Saving converted preset");
                PresetLoader.SavePreset(preset, options.SaveConvertedPresetPath);
            }

            if (options.ConvertPresetAndExit)
            {
                if (!string.IsNullOrWhiteSpace(options.SaveConvertedPresetPath))
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
            if (!string.IsNullOrWhiteSpace(options.SaveConvertedPresetPath))
            {
                Log.Warning("Cannot save converted preset when input is not a legacy preset");
            }

            if (options.ConvertPresetAndExit)
            {
                Log.Warning("[ConvertPresetAndExit] No change has been made to the preset, exiting");
                return 1;
            }
        }

        OverwriteOutput(preset.Output, options);
        if (!VerifyOutput(preset.Output))
        {
            Log.Error("Output configuration is invalid");
            return 1;
        }

        Log.Information("Starting filtering for preset: {Preset}", preset.Name);
        var songSource = GetSongSource(preset.Input);
        var allSongs = songSource.GetSongs();
        var filteredSongs = await FilterSongsAsync(allSongs, preset);
        if (filteredSongs == null)
        {
            Log.Error("Failed to filter songs");
            return -1;
        }

        var count = OutputSongs(filteredSongs, preset);
        Log.Information("Filtered songs: {Count}", count);
        return 0;
    }

    private void OverwriteOutput(OutputConfig output, Options options)
    {
        if (options.DisablePlaylistOutput)
        {
            output.SavePlaylist = false;
        }
        else if (output.SavePlaylist)
        {
            if (!string.IsNullOrWhiteSpace(options.OutputPlaylist))
            {
                output.PlaylistPath = options.OutputPlaylist;
            }

            output.SavePlaylist = !string.IsNullOrWhiteSpace(output.PlaylistPath);
        }

        if (options.DisableSongDownload)
        {
            output.DownloadSongs = false;
        }
        else if (output.DownloadSongs)
        {
            if (!string.IsNullOrWhiteSpace(options.OutputSongPath))
            {
                output.DownloadPath = options.OutputSongPath;
            }

            output.DownloadSongs = !string.IsNullOrWhiteSpace(output.DownloadPath);
        }
    }

    private bool VerifyOutput(OutputConfig output)
    {
        if (output.SavePlaylist)
        {
            if (string.IsNullOrWhiteSpace(output.PlaylistPath))
            {
                Log.Warning("Playlist output is enabled but no path is specified");
            }
            else if (!Directory.Exists(output.PlaylistPath))
            {
                Log.Error("Playlist output path doesn't exist or is not a directory: {Path}", output.PlaylistPath);
                return false;
            }
        }

        if (output.DownloadSongs)
        {
            if (string.IsNullOrWhiteSpace(output.DownloadPath))
            {
                Log.Warning("Song download is enabled but no path is specified");
            }
            else if (!Directory.Exists(output.DownloadPath))
            {
                Log.Error("Song download path doesn't exist or is not a directory: {Path}", output.DownloadPath);
                return false;
            }
        }

        return true;
    }
}