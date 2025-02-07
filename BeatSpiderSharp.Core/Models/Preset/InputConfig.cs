using BeatSpiderSharp.Core.Models.Preset.Enums;

namespace BeatSpiderSharp.Core.Models.Preset;

public class InputConfig
{
    public SongInputSource Source { get; set; } = SongInputSource.SongDetailsCache;

    public IList<string> Playlists { get; set; } = new List<string>();

    public IList<string> ManualInput { get; set; } = new List<string>();
}