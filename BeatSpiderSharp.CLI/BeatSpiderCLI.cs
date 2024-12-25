using BeatSpiderSharp.Core;
using Serilog;

namespace BeatSpiderSharp.CLI;

public class BeatSpiderCLI : BeatSpider
{
    protected override void ConfigureLogger(LoggerConfiguration configuration)
    {
        base.ConfigureLogger(configuration);
        configuration.WriteTo.Console();
    }
    
    public void Run()
    {
        Log.Information("BeatSpiderCLI!");
        var presetPath = @"T:\test\BeatSpider-3.3.4.0（zeyu整理曲包用的软件）\Settings\演示_填写所有选项与高级搜索演示.brset";

        var preset = LoadLegacyPreset(presetPath);
        
        if (preset == null)
        {
            Log.Warning("Failed to load preset");
            return;
        }
        
        Log.Information("Song source: {Source}", preset.SongSource);

        WriteLegacyPreset(preset, "./output.json");
    }
}