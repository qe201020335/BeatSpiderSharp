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
            WordBreaking = WordBreaking.Standard,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Scale font to fit image
        Log.Debug("Scaling font to fit image");
        ScaleFont(name, image, options);
        Log.Debug("Scaled font size: {Size}", options.Font.Size);

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

    // might be a bit overkill to find multiple sizes, but it works great
    private static void ScaleFont(string text, Image image, RichTextOptions options)
    {
        var minSize = (float) Math.Sqrt(image.Width * image.Height) * 0.1f; // smallest font size before it is too small to read
        var maxSize = (float) Math.Sqrt(image.Width * image.Height) * 0.25f; // too large it looks bad
        Log.Debug("Min size: {Min}, Max size: {Max}", minSize, maxSize);
        float newSize;
        
        // find the smallest relative size that fits the text in one line without word wrapping
        for (var rel = 0.8f; rel <= 0.95; rel += 0.01f)
        {
            newSize = GetFontSize(rel, image, text, options, false);
            if (newSize >= minSize)
            {
                // text size is ok for one line
                Log.Debug("rel: {Size}", rel);
                newSize = Math.Min(newSize, maxSize);
                options.Font = new Font(options.Font, newSize);
                return;
            }
        }
        
        // one line doesn't fit, find a relative size that fits with word wrapping
        for (var rel = 0.85f; rel <= 0.97; rel += 0.01f)
        {
            newSize = GetFontSize(rel, image, text, options, true);
            if (newSize >= minSize)
            {
                // text size is ok with word wrapping
                Log.Debug("rel: {Size}", rel);
                newSize = Math.Min(newSize, maxSize);
                options.Font = new Font(options.Font, newSize);
                return;
            }
        }
        
        // text is still too large
        // nothing more to do, just need to make it fit
        newSize = GetFontSize(0.98f, image, text, options, true);
        options.Font = new Font(options.Font, newSize);
        Log.Warning("Text is too long, might be too small to read");
    }

    private static float GetFontSize(float relativeSize, Image image, string text, RichTextOptions options, bool wrap)
    {
        options.WrappingLength = wrap ? image.Width * relativeSize : -1;
        var rect = TextMeasurer.MeasureSize(text, options);
        var scalingFactor = Math.Min(image.Width * relativeSize / rect.Width, image.Height * relativeSize / rect.Height);
        return scalingFactor * options.Font.Size;
    }
}