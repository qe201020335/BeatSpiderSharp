using System.Text;
using BeatSpiderSharp.Core.Models;
using Newtonsoft.Json;
using Serilog;
using SongDetailsCache;

namespace BeatSpiderSharp.Core;

public abstract class BeatSpider
{
    protected readonly JsonSerializer LegacySerializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
#if DEBUG
        MissingMemberHandling = MissingMemberHandling.Error
#endif
    });
    
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
        Log.Information("Loading legacy preset from {Path}", path);
        if (!File.Exists(path))
        {
            Log.Error("File {Path} does not exist", path);
            return null;
        }

        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var jsonReader = new JsonTextReader(reader);
        return LegacySerializer.Deserialize<LegacyPreset>(jsonReader);
    }

    public void WriteLegacyPreset(LegacyPreset preset, string path)
    {
        Log.Information("Writing legacy preset to {Path}", path);
        using var outputStream = new FileStream(path, FileMode.Create);
        using var textWriter = new StreamWriter(outputStream, Encoding.UTF8);
        using var jsonWriter = new JsonTextWriter(textWriter);
        LegacySerializer.Serialize(jsonWriter, preset);
    }
}