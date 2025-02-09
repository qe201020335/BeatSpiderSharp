using System.Reflection;
using BeatSaberPlaylistsLib.Legacy;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BeatSpiderSharp.Core;

public class PlaylistExporter
{
    public bool PostProcess { get; init; }

    public void Export(IEnumerable<BeatSpiderSong> songs, string title, string author, string? description, string targetPath)
    {
        Log.Information("Exporting playlist {Name}", title);
        var playlist = new LegacyPlaylist(title, title, string.IsNullOrWhiteSpace(author) ? null : author)
        {
            Description = description, 
            ReadOnly = true
        };

        using var coverStream = GetCover(title);
        playlist.SetCover(coverStream);

        // TODO add preset to playlist
        // playlist.SetCustomData("BeatSpiderSharpPreset", "TODO");

        foreach (var song in songs)
        {
            playlist.Add(song.Hash, song.SongDetails.songName, song.Bsr, null);
        }
        
        Log.Debug("Saving playlist {Name} to {Target}", title, targetPath);

        JsonSerializerSettings settings;
        object obj;
        if (PostProcess)
        {
            settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };
            var jobj = JObject.FromObject(playlist);
            var descProp = jobj.Property("playlistTitle");
            if (descProp != null)
            {
                var jsongs = jobj.Property("songs");
                if (jsongs != null)
                {
                    jsongs.Remove();
                    descProp.AddBeforeSelf(jsongs);
                }
            }
            obj = jobj;
        }
        else
        {
            settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            obj = playlist;
        }
        var serializer = JsonSerializer.Create(settings);
        serializer.Serialize(obj, targetPath);
    }

    private Stream GetCover(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("BeatSpiderSharp.Core.Assets.cover.png");
        if (stream == null)
        {
            throw new NullReferenceException("Could not find cover.png in resources");
        }

        return stream;
    }
}