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
    private readonly IPlaylist? _playlist;

    private readonly DirectoryInfo _playlistFolder = new(Path.GetFullPath("TemporaryPlaylists"));

    public PlaylistSongs(string playlistPath, SongDetails songDetails) : base(songDetails)
    {
        Log.Debug("Loading playlist: {PlaylistPath}", playlistPath);
        if (!File.Exists(playlistPath))
        {
            Log.Warning("Playlist file not found: {PlaylistPath}", playlistPath);
        }

        // We can't use the given playlistPath's folder directly,
        // because PlaylistManager will recursively create child managers for all the subfolders
        _playlistFolder.Create();
        var playlistName = Path.GetFileName(playlistPath);
        File.Copy(playlistPath, Path.Combine(_playlistFolder.FullName, playlistName), true);

        // TODO Inject a PlaylistManager instance for "temp" playlist loading
        // TODO Use a manager for temp files instead of hardcoding the folder
        var playlistManager = new PlaylistManager(_playlistFolder.FullName, new LegacyPlaylistHandler(), new BlistPlaylistHandler());

        try
        {
            _playlist = playlistManager.GetPlaylist(playlistName, false);
        }
        catch (Exception e)
        {
            _playlist = null;
            Log.Error(e, "Failed to load playlist: {Name}", playlistName);
        }
    }

    protected override IEnumerable<Song> GetSongDetailSongs()
    {
        if (_playlist == null)
        {
            Log.Warning("Playlist is not loaded");
            return [];
        }

        var result = _playlist.Select(entry =>
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
            .SelectNotNull();
        return result;
    }
}