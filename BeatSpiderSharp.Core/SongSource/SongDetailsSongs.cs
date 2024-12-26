using BeatSpiderSharp.Core.Interfaces;
using BeatSpiderSharp.Core.Models;
using Serilog;
using SongDetailsCache;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.SongSource;

public class SongDetailsSongs(SongDetails songDetails) : ISongSource
{
    protected SongDetails SongDetails => songDetails;
    public bool ReverseOrder { get; init; }

    public IEnumerable<BeatSpiderSong> GetSongs()
    {
        return ReverseOrder 
            ? GetSongDetailSongs().Reverse().Select(BeatSpiderSong.FromSongDetailsSong)
            : GetSongDetailSongs().Select(BeatSpiderSong.FromSongDetailsSong);
    }

    protected virtual IEnumerable<Song> GetSongDetailSongs()
    {
        return SongDetails.songs;
    }

    protected Song? GetSongByHash(string hash)
    {
        if (songDetails.songs.FindByHash(hash, out var song))
        {
            return song;
        }

        Log.Warning("Song with hash {Hash} not found", hash);
        return null;
    }

    protected Song? GetSongByBsr(string bsr)
    {
        if (songDetails.songs.FindByMapId(bsr, out var song))
        {
            return song;
        }

        Log.Warning("Song with bsr {Bsr} not found", bsr);
        return null;
    }
}