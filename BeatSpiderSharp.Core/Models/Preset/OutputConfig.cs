namespace BeatSpiderSharp.Core.Models.Preset;

public class OutputConfig
{
    public int MaxSongs { get; set; } = 0;

    public bool SavePlaylist { get; set; } = false;

    public string PlaylistPath { get; set; } = string.Empty;

    public bool DownloadSongs { get; set; } = false;

    public string DownloadPath { get; set; } = string.Empty;

    public bool SkipExisting { get; set; } = false;

    public IList<string> ExistingSongPaths { get; set; } = new List<string>();
    
    public bool CopyLocalSongs { get; set; } = false;
    
    public IList<string> LocalSongPaths { get; set; } = new List<string>();
}