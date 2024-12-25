using BeatSpiderSharp.Core.Models;

namespace BeatSpiderSharp.Core.Interfaces;

public interface ISongSource
{
    IEnumerable<BeatSpiderSong> GetSongs();
}