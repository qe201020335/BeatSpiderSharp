using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeatSpiderSharp.Core.Models;
using Serilog;
using SongDetailsCache;

namespace BeatSpiderSharp.Core;

public abstract class BeatSpider
{
    protected readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#if DEBUG
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
#endif
    };
    
    protected SongDetails SongDetails { get; private set; } = null!;

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

    public LegacyPreset? LoadLegacyPreset(string path)
    {
        if (!File.Exists(path))
        {
            Log.Error("File {Path} does not exist", path);
            return null;
        }

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<LegacyPreset>(stream, JsonOptions);
    }

    public void WriteLegacyPreset(LegacyPreset preset, string path)
    {
        using var outputStream = new FileStream(path, FileMode.Create);
        JsonSerializer.Serialize(outputStream, preset, JsonOptions);
    }
}