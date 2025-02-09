using System.Reflection;
using BeatSaberPlaylistsLib.Legacy;
using BeatSpiderSharp.Core.Models;
using BeatSpiderSharp.Core.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

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

    private static Stream GetCover(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var coverStream = assembly.GetManifestResourceStream("BeatSpiderSharp.Core.Assets.cover.png");
        using var fontStream = assembly.GetManifestResourceStream("BeatSpiderSharp.Core.Assets.font.ttf");
        if (coverStream == null || fontStream == null)
        {
            throw new NullReferenceException("Could not find cover.png or font.ttf in resources");
        }

        // Unlikely to happen, but just in case
        if (string.IsNullOrWhiteSpace(name)) return coverStream;

        // Load image
        using var image = Image.Load(coverStream);
        coverStream.Dispose();
        
        // Load font
        FontCollection collection = new();
        var family = collection.Add(fontStream);
        Log.Debug("Using font {Name} ", family.Name);
        var font = family.CreateFont(72, FontStyle.Regular);
        
        RichTextOptions options = new(font)
        {
            Origin = new PointF(image.Width / 2f, image.Height / 2f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Scale font to fit image
        var rect = TextMeasurer.MeasureSize(name, options);
        var scalingFactor = image.Width * 0.8f / rect.Width;
        var scaledFont = new Font(font, scalingFactor * font.Size);
        options.Font = scaledFont;

        // Draw text
        image.Mutate(ctx =>
        {
            ctx.DrawText(options, name, Color.White);
        });
        
        // Save image
        var encoder = new JpegEncoder { Quality = 100 };
#if DEBUG
        image.SaveAsJpeg("cover-generated.jpg", encoder);
#endif
        var buffer = new MemoryStream();
        image.SaveAsJpeg(buffer, encoder);
        return buffer;
    }
}