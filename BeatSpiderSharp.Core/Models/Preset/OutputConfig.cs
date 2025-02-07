namespace BeatSpiderSharp.Core.Models.Preset;

public class OutputConfig
{
    public bool LimitSongs { get; set; }

    public int? MaxSongs { get; set; }

    public bool SavePlaylist { get; set; }

    public string PlaylistPath { get; set; } = string.Empty;

    public bool DownloadSongs { get; set; }

    public string DownloadPath { get; set; } = string.Empty;

    public bool SkipExisting { get; set; }

    public IList<string> ExistingSongPaths { get; set; } = new List<string>();
    
    public bool CopyLocalSongs { get; set; }
    
    public IList<string> LocalSongPaths { get; set; } = new List<string>();
}