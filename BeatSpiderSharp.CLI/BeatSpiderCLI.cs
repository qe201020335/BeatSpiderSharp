using System.IO.Compression;
using BeatSpiderSharp.Core;
using BeatSpiderSharp.Core.SongSource;
using BeatSpiderSharp.Core.Utilities;
using BeatSpiderSharp.Extensions;
using BeatSpiderSharp.Legacy;
using BeatSpiderSharp.Models;
using BeatSpiderSharp.Models.BeatSaver;
using BeatSpiderSharp.Models.Enums;
using BeatSpiderSharp.Models.Preset;
using BeatSpiderSharp.Shared;
using Serilog;

namespace BeatSpiderSharp.CLI;

public class BeatSpiderCLI(bool verbose) : BeatSpider(verbose)
{
    protected override void ConfigureLogger(LoggerConfiguration configuration)
    {
        base.ConfigureLogger(configuration);

        if (Verbose)
        {
            configuration.MinimumLevel.Verbose();
        }
#if DEBUG
        else
        {
            configuration.MinimumLevel.Debug();
        }
#endif
        configuration
            // .WriteTo.File("BeatSpiderCLI.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console();
    }

    public async Task<int> RunAsync(BeatSpiderOptions options, CancellationToken cToken)
    {
        Log.Information("BeatSpiderCLI!");
        Log.Information("Version: {Version}", Constants.Version.ToFullString());

#if DEBUG
        Log.Debug("Options: {@Options}", options);
#endif

        Preset preset;

        if (options.InputIsLegacy)
        {
            // allow empty author
            var author = options.PresetAuthor ?? Environment.UserName;

            try
            {
                var p = LegacyPresetLoader.LoadAndConvertLegacyPreset(options.InputPreset, author);
                if (p == null)
                {
                    Log.Error("Failed to load legacy preset");
                    return 1;
                }

                preset = p;
            }
            catch (LegacyConversionException e)
            {
                Log.Error(e.InnerException, "Failed to convert legacy preset: {Message}", e.Message);
                Log.Debug(e, "Legacy conversion exception details");
                return 1;
            }
            catch (Exception e)
            {
                Log.Error(e, "Unexpected error while loading legacy preset: {Message}", e.Message);
                return 1;
            }

            cToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(options.SaveConvertedPresetPath))
            {
                Log.Information("Saving converted preset");
                PresetLoader.SavePreset(preset, options.SaveConvertedPresetPath);
            }

            cToken.ThrowIfCancellationRequested();

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
            cToken.ThrowIfCancellationRequested();

            if (p == null)
            {
                Log.Error("Failed to load preset");
                return 1;
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

        OverwriteOptions(preset, options);
        if (!VerifyOutput(preset.Output))
        {
            Log.Error("Output configuration is invalid");
            return 1;
        }

        // load songs
        if (!File.Exists(options.SongCachePath))
        {
            Log.Error("Song cache file not found");
            return 1;
        }

        Log.Information("Loading songs from cached data");
        var cacheFileStream = File.OpenRead(options.SongCachePath);
        // Disposing the GZipStream disposes the file stream underneath it, so only the outermost needs a handle.
        await using Stream songDataStream = options.GZipCacheData
            ? new GZipStream(cacheFileStream, CompressionMode.Decompress)
            : cacheFileStream;

        Log.Information("Loading song inputs");
        var allSongs = JsonExtensions
            .DeserializeArrayAsync(songDataStream, BeatSaverJsonContext.Default.Song, ["docs"], cToken)
            .Where(BeatSpiderSong.ValidateBeatSaverSong)
            .Select(BeatSpiderSong.FromBeatSaverSong);

        using var songSourceFactory = new SongSourceFactory();
        try
        {
            allSongs = preset.Input.Source switch
            {
                SongInputSource.Playlists => await songSourceFactory.CreateFromPlaylists(preset.Input.Playlists,
                    allSongs, cToken),
                SongInputSource.ManualInput => SongSourceFactory.CreateFromManualSongInput(preset.Input.ManualInput,
                    allSongs),
                _ => allSongs
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load song input sources");
            return 1;
        }

        Log.Information("Starting filtering for preset: {Preset}", preset.Name);
        var filteredSongs = FilterSongs(allSongs, preset);

        var count = await OutputSongsAsync(filteredSongs, preset, cToken);
        Log.Information("Filtered songs: {Count}", count);
        return 0;
    }

    private void OverwriteOptions(Preset preset, BeatSpiderOptions options)
    {
        preset.Author = options.PresetAuthor ?? preset.Author;

        var output = preset.Output;
        if (options.DisablePlaylistOutput)
        {
            output.Playlist.SavePlaylist = false;
        }
        else if (output.Playlist.SavePlaylist)
        {
            if (!string.IsNullOrWhiteSpace(options.PlaylistDirectory))
            {
                output.Playlist.PlaylistDirectory = options.PlaylistDirectory;
            }

            output.Playlist.SavePlaylist = !string.IsNullOrWhiteSpace(output.Playlist.PlaylistDirectory);
        }

        if (options.DisableSongDownload)
        {
            output.SongDownload.DownloadSongs = false;
        }
        else if (output.SongDownload.DownloadSongs)
        {
            if (!string.IsNullOrWhiteSpace(options.OutputSongPath))
            {
                output.SongDownload.DownloadPath = options.OutputSongPath;
            }

            output.SongDownload.DownloadSongs = !string.IsNullOrWhiteSpace(output.SongDownload.DownloadPath);

            if (options.SkipExistingSongs)
            {
                if (!output.SongDownload.SkipExisting)
                {
                    // we don't want leftover paths to be in effect when skipping was not enabled
                    output.SongDownload.ExistingSongPaths.Clear();
                }

                output.SongDownload.SkipExisting = true;
                output.SongDownload.ExistingSongPaths.Add(output.SongDownload.DownloadPath);
            }

            if (!string.IsNullOrEmpty(options.LocalZipsPath))
            {
                output.SongDownload.LocalZipsPath = options.LocalZipsPath;
            }

            if (options.SaveSongZips.HasValue)
            {
                output.SongDownload.SaveZips = options.SaveSongZips.Value;
            }

            if (options.UseLocalZips.HasValue)
            {
                output.SongDownload.UseLocalZips = options.UseLocalZips.Value;
            }
        }
    }

    private bool VerifyOutput(OutputConfig output)
    {
        if (output.Playlist.SavePlaylist)
        {
            if (string.IsNullOrWhiteSpace(output.Playlist.PlaylistDirectory))
            {
                Log.Warning("Playlist output is enabled but no path is specified");
            }
            else if (!Directory.Exists(output.Playlist.PlaylistDirectory))
            {
                Log.Error("Playlist output path doesn't exist or is not a directory: {Path}",
                    output.Playlist.PlaylistDirectory);
                return false;
            }
        }

        if (output.SongDownload.DownloadSongs)
        {
            if (string.IsNullOrWhiteSpace(output.SongDownload.DownloadPath))
            {
                Log.Warning("Song download is enabled but no path is specified");
            }
            else if (!Directory.Exists(output.SongDownload.DownloadPath))
            {
                Log.Error("Song download path doesn't exist or is not a directory: {Path}",
                    output.SongDownload.DownloadPath);
                return false;
            }
        }

        return true;
    }
}
