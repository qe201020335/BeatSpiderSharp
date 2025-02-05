namespace BeatSpiderSharp.CLI.Command;

public record Options
{
    public string InputPreset { get; init; } = "";
    
    public string? OutputPlaylist { get; init; }
    
    public string? OutputSongPath { get; init; }
    
    public bool DisablePlaylistOutput { get; init; }
    
    public bool DisableSongDownload { get; init; }
    
    public bool Verbose { get; init; }
}