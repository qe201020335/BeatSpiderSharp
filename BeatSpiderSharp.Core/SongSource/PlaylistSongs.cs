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
    private readonly IList<string> _keys;

    public PlaylistSongs(IList<string> playlistPaths, SongDetails songDetails, string tempRoot) : base(songDetails)
    {
        if (playlistPaths.Count == 0)
        {
            Log.Warning("No playlist paths given");
            _keys = [];
            return;
        }

        var playlistFolder = new DirectoryInfo(Path.Combine(tempRoot, "playlists_" + Path.GetRandomFileName()));
        Log.Debug("Using temporary folder for playlists: {PlaylistFolder}", playlistFolder.FullName);
        playlistFolder.Create();
        var pm = new PlaylistManager(playlistFolder.FullName, new LegacyPlaylistHandler(), new BlistPlaylistHandler());

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

            var playlistName = Path.GetFileName(path);
            try
            {
                File.Copy(path, Path.Combine(playlistFolder.FullName, playlistName), true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to copy playlist for loading: {Name}", playlistName);
                continue;
            }

            try
            {
                var playlist = pm.GetPlaylist(playlistName, false);
                if (playlist == null)
                {
                    Log.Warning("Playlist is loaded as null: {Name}", playlistName);
                }
                else
                {
                    playlists.Add(playlist);
                    songCount += playlists.Count;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load playlist: {Name}", playlistName);
            }
        }

        var keys = new List<string>(songCount);
        var deduped = playlists.SelectMany(GetKeysFromPlaylist).Distinct();
        keys.AddRange(deduped);
        _keys = keys;
        playlistFolder.Delete(true);
        Log.Debug("Loaded {SongCount} songs from {PlaylistCount} playlists", keys.Count, playlists.Count);
    }

    private IEnumerable<string> GetKeysFromPlaylist(IPlaylist playlist)
    {
        foreach (var song in playlist)
        {
            if (!string.IsNullOrWhiteSpace(song.Key))
            {
                yield return song.Key;
            }
            else if (!string.IsNullOrWhiteSpace(song.Hash))
            {
                var songDetailsSong = GetSongByHash(song.Hash);
                if (songDetailsSong != null)
                {
                    yield return songDetailsSong.Value.key;
                }
            }
            else
            {
                Log.Warning("Playlist entry with no hash or key encountered. Skipping");
            }
        }
    }

    protected override IEnumerable<Song> GetSongDetailSongs()
    {
        if (_keys.Count == 0)
        {
            Log.Warning("No songs are loaded from playlists");
            return [];
        }

        var result = _keys.Select(GetSongByBsr).SelectNotNull();
        return result;
    }
}