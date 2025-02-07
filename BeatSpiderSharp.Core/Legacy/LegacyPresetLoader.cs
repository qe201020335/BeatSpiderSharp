using BeatSpiderSharp.Core.Models.Preset;
using Newtonsoft.Json;
using Serilog;
using BeatSpiderSharp.Core.Utilities.Extensions;

namespace BeatSpiderSharp.Core.Legacy;

public static class LegacyPresetLoader
{
    private static readonly JsonSerializer LegacySerializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
#if DEBUG
        MissingMemberHandling = MissingMemberHandling.Error
#endif
    });
    
    public static LegacyPreset? LoadLegacyPreset(string path)
    {
        Log.Information("Loading legacy preset from {Path}", path);
        return LegacySerializer.DeserializeObject<LegacyPreset>(path);
    }

    public static void SaveLegacyPreset(LegacyPreset preset, string path)
    {
        Log.Information("Writing legacy preset to {Path}", path);
        LegacySerializer.Serialize(preset, path);
    }

    public static Preset ConvertToPreset(this LegacyPreset legacyPreset)
    {
        //TODO
        return new Preset();
    }
}