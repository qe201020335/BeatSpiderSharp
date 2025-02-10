using BeatSaverSharp;
using BeatSpiderSharp.Core.Interfaces;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.Models.Preset;

namespace BeatSpiderSharp.Core.Filters;

public class RootFilter: ISongFilter
{
    private readonly FilterConfig _config;
    
    private IList<ISongFilter> _filters = new List<ISongFilter>(1);  // there is currently only one sub filter existing
    
    public bool LogExclusions { get; init; }
    
    public RootFilter(FilterConfig config)
    {
        _config = config;
    }
    
    public async Task InitAsync(BeatSaver beatSaver)
    {
        var filter = new DetailFilter(_config.DetailFilter) { LogExclusions = LogExclusions };
        await filter.InitAsync(beatSaver);
    }
    
    public bool FilterSong(BeatSpiderSong song)
    {
        return _filters.Count == 0 || _filters.All(filter => filter.FilterSong(song));
    }
}