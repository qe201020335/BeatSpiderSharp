using BeatSpiderSharp.Core.Models.Preset;
using BeatSpiderSharp.Core.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace BeatSpiderSharp.Core.Utilities;

public static class PresetLoader
{
    private static readonly JsonSerializer PresetSerializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Converters = [new StringEnumConverter()]
    });
    
    public static Preset? LoadPreset(string path)
    {
        Log.Information("Loading preset from {Path}", path);
        return PresetSerializer.DeserializeObject<Preset>(path);
    }

    public static void SavePreset(Preset preset, string path)
    {
        Log.Information("Writing preset to {Path}", path);
        PresetSerializer.Serialize(preset, path);
    }
}