using BeatSpiderSharp.Core;
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
            .WriteTo.Console();
    }
    
    public async Task Run()
    {
        Log.Information("BeatSpiderCLI!");

        await InitAsync();
        
        var presetPath = @"T:\test\BeatSpider-3.3.4.0（zeyu整理曲包用的软件）\Settings\演示_填写所有选项与高级搜索演示.brset";

        var preset = LoadLegacyPreset(presetPath);
        
        if (preset == null)
        {
            Log.Warning("Failed to load preset");
            return;
        }
        
        Log.Information("Song source: {Source}", preset.SongSource);
        
        Log.Information("Loading SongDetails");

        var songDetails = await SongDetails.Init();
        
        ISongSource songSource = new ManualSongInput(preset.ManualSongInput.Songs, songDetails);
        
        var songs = songSource.GetSongs().DistinctBy(song => song.Hash).ToArray();
        
        Log.Information("Loaded {Count} songs from manual input source:\n{Songs}", songs.Length, songs);

        songSource = new PlaylistSongs(preset.PlaylistInput.Path, songDetails);
        
        songs = songSource.GetSongs().DistinctBy(song => song.Hash).ToArray();
        
        Log.Information("Loaded {Count} songs from playlist:\n{Songs}", songs.Length, songs);
        
        // var allSongs = new SongDetailsSongs(songDetails).GetSongs();
        // var count = 0;
        // var time = DateTime.Now;
        // foreach (var song in allSongs)
        // {
        //     count++;
        //     Log.Debug("{Song}",  song);
        // }
        //
        // var elapsed = DateTime.Now - time;
        // Log.Information("Looped through {Count} songs from SongDetails in {Elapsed}", count, elapsed);
        
        // WriteLegacyPreset(preset, "./output.json");
    }
}