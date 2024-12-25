using SongDetailsSong = SongDetailsCache.Structs.Song;

namespace BeatSpiderSharp.Core.Models;

public class BeatSpiderSong
{
    public string Hash { get; init; } = string.Empty;
    
    public string Bsr { get; init; } = string.Empty;
    
    public SongDetailsSong SongDetails { get; init; }
    
    // public BeatSaverSharp.Models.Beatmap? Beatmap { get; set; }
    
    // TODO add more info if needed

    public static BeatSpiderSong FromSongDetailsSong(SongDetailsSong song)
    {
        return new BeatSpiderSong
        {
            Hash = song.hash,
            Bsr = song.key,
            SongDetails = song
        };
    }
    
    public override string ToString()
    {
        return $"{Bsr} ({SongDetails.songName} - {SongDetails.levelAuthorName})";
    }
}