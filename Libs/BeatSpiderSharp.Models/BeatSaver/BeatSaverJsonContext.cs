using System.Text.Json.Serialization;

namespace BeatSpiderSharp.Models.BeatSaver;

/// <summary>
/// Source-generated metadata for the BeatSaver cache models. Deserialization only - these models are read from
/// the song cache and never written back out.
/// </summary>
/// <remarks>
/// Using the generator instead of reflection is the point: the song cache holds the entire BeatSaver database, so
/// the per-property binding cost is paid once per map, times hundreds of thousands of maps.
/// </remarks>
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Song))]
public partial class BeatSaverJsonContext : JsonSerializerContext;
