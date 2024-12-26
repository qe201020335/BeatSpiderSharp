using BeatSpiderSharp.Core.Models;

namespace BeatSpiderSharp.Core.Interfaces;

public interface ISongFilter
{
    IEnumerable<BeatSpiderSong> Filter(IEnumerable<BeatSpiderSong> songs);
}