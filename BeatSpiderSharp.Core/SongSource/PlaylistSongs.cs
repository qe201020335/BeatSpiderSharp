using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using BeatSpiderSharp.Core.Utilities.Extensions;
using Serilog;
using SongDetailsCache;
using SongDetailsCache.Structs;

namespace BeatSpiderSharp.Core.SongSource;

public class PlaylistSongs : SongDetailsSongs
{
    private readonly IList<IPlaylistSong> _songs;

    public PlaylistSongs(IList<string> playlistPaths, SongDetails songDetails, string tempRoot) : base(songDetails)
    {
        if (playlistPaths.Count == 0)
        {
            Log.Warning("No playlist paths given");
            _songs = [];
            return;
        }

        var bplistHandler = new LegacyPlaylistHandler();
        var blistHandler = new BlistPlaylistHandler();
        var playlists = new List<IPlaylist>(playlistPaths.Count);
        var songCount = 0;
        foreach (var path in playlistPaths)
        {
            Log.Debug("Loading playlist: {PlaylistPath}", path);
            if (!File.Exists(path))
            {
                Log.Warning("Playlist file not found: {PlaylistPath}", path);
                continue;
            }

            var extension = Path.GetExtension(path);

            if (string.IsNullOrWhiteSpace(extension))
            {
                Log.Error("Playlist file has no extension: {PlaylistPath}", path);
            }

            try
            {
                var playlist = extension switch
                {
                    ".json" or ".bplist" => bplistHandler.Deserialize(path),
                    ".blist" => blistHandler.Deserialize(path),
                    _ => null
                };

                if (playlist == null)
                {
                    Log.Error("Playlist format is unknown or is null: {Name}", path);
                }
                else
                {
                    playlists.Add(playlist);
                    songCount += playlist.Count;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load playlist: {Name}", path);
            }
        }

        var songs = new List<IPlaylistSong>(songCount);
        var concat = playlists.SelectMany(playlist => playlist);
        songs.AddRange(concat);
        _songs = songs;
        Log.Information("Loaded {SongCount} songs from {PlaylistCount} playlists", _songs.Count, playlists.Count);
    }

    protected override IEnumerable<Song> GetSongDetailSongs()
    {
        if (_songs.Count == 0)
        {
            Log.Warning("No songs are loaded from playlists");
            return [];
        }

        var result = _songs.Select(entry =>
            {
                if (!string.IsNullOrEmpty(entry.Hash))
                {
                    return GetSongByHash(entry.Hash);
                }

                if (!string.IsNullOrEmpty(entry.Key))
                {
                    return GetSongByBsr(entry.Key);
                }

                Log.Warning("Playlist entry with no hash or key encountered. Skipping");

                return null;
            })
            .SelectNotNull()
            .DistinctBy(song => song.key);
        return result;
    }
}