using System.Diagnostics;
using System.IO.Compression;
using BeatSpiderSharp.Extensions;
using BeatSpiderSharp.Models.BeatSaver;
using JsonTest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

Console.WriteLine("Hello, World!");
if (args.Length == 0)
{
    Console.WriteLine("Missing json.gz path argument");
    Environment.Exit(1);
}

var path = args[0];
if (!File.Exists(path))
{
    Console.WriteLine($"File {path} does not exist");
    Environment.Exit(1);
}
var jsonReader = new JsonTextReader(new StreamReader(new GZipStream(File.OpenRead(path), CompressionMode.Decompress)));
var serializer = JsonSerializer.Create(new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    // Uncomment this to check whether there are new fields in the json that are not in the model
    // MissingMemberHandling = MissingMemberHandling.Error
});


var watch = Stopwatch.StartNew();

var memoryPeak = GC.GetTotalMemory(false);
var i = 0;
var characteristics = new HashSet<string>();
var difficulties = new HashSet<string>();
var (reduced, sum) = await serializer
    .DeserializeArrayAsync<JObject>(jsonReader, ["docs"])
    .AggregateAsync(((JToken?)null, 0d), (acc, obj) =>
{
    if (obj is null) return acc;
    var (reduced, sum) = acc;
    reduced = JsonMapper.MergeJToken(obj, reduced);
    try
    {
        var song = obj.ToObject<Song>(serializer)!;
        foreach (var level in song.LatestVersion.Diffs)
        {
            if (!string.IsNullOrWhiteSpace(level.Characteristic))
            {
                characteristics.Add(level.Characteristic);
            }

            if (!string.IsNullOrWhiteSpace(level.Difficulty))
            {
                difficulties.Add(level.Difficulty);
            }
        }

        if (song.Stats?.Plays != 0)
        {
            Console.WriteLine($"{song.Id} Plays: {song.Stats!.Plays}");
        }
        
        sum += song.Id?.Length ?? 0;
        sum += song.Metadata?.SongName?.Length ?? 0;
        sum += song.Metadata?.SongAuthorName?.Length ?? 0;
        sum += song.Metadata?.LevelAuthorName?.Length ?? 0;

        var latest = song.Versions[0];
        sum += latest.Hash?.Length ?? 0;

        var diff = latest.Diffs[0];
        sum += diff.Difficulty?.Length ?? 0;
        sum += diff.Characteristic?.Length ?? 0;
        sum += diff.Stars;
        sum += diff.BlStars;
        sum += diff.Notes;
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine(obj.ToString());
        Environment.Exit(1);
    }
    i++;
    memoryPeak = Math.Max(memoryPeak, GC.GetTotalMemory(false));
    return (reduced, sum);
});

watch.Stop();
Console.WriteLine(
    $"Time taken to merge json and query 10 values each from {i} songs: {watch.ElapsedMilliseconds / 1000f} s");
Console.WriteLine($"Sum: {sum}");

Console.WriteLine($"Peak memory usage: {memoryPeak / 1024 / 1024} MiB");
Console.WriteLine($"Unique characteristics: {string.Join(", ", characteristics)}");
Console.WriteLine($"Unique difficulties: {string.Join(", ", difficulties)}");
var objMap = JsonMapper.MapJToken(reduced);
Console.WriteLine("Json map:");
Console.WriteLine(objMap);
