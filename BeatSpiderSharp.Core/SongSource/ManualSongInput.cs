using BeatSpiderSharp.Core.Utilities.Extensions;
using Serilog;
using SongDetailsCache;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.SongSource;

public class ManualSongInput(IList<string> input, SongDetails songDetails) : SongDetailsSongs(songDetails)
{
    protected override IEnumerable<Song> GetSongDetailSongs()
    {
        Log.Debug("Processing manual input: {Input}", input);

        var result = input.Where(entry => !string.IsNullOrWhiteSpace(entry)).Select(entry =>
            {
                if (!entry.IsHex())
                {
                    Log.Warning("Invalid entry: {Entry}", entry);
                    return null;
                }

                if (entry.Length == 40) // hash
                {
                    return GetSongByHash(entry);
                }

                return GetSongByBsr(entry);
            })
            .SelectNotNull();

        return result;
    }
}