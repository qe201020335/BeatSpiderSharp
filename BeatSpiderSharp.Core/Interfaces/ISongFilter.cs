using BeatSpiderSharp.Core.Models;

namespace BeatSpiderSharp.Core.Interfaces;

public interface ISongFilter
{
    bool FilterSong(BeatSpiderSong song);
}