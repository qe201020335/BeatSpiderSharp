using BeatSpiderSharp.Core.Models;

namespace BeatSpiderSharp.Core.Interfaces;

public interface ISongFilter
{
    bool LogExclusions { get; init; }
    
    bool FilterSong(BeatSpiderSong song);
}