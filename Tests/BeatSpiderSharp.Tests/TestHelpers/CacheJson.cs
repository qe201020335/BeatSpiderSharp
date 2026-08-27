using System.Text;

namespace BeatSpiderSharp.Tests.TestHelpers;

/// <summary>
/// Builds song-cache payloads in the shape the Cacher writes: <c>{"docs":[...],"date":N}</c>.
/// </summary>
public static class CacheJson
{
    /// <summary>
    /// A single BeatSaver document. Only fields the models map are emitted, so the DEBUG unmapped-member check
    /// does not trip.
    /// </summary>
    public static string Song(int i, int descriptionLength = 30)
    {
        var description = new string((char)('a' + i % 26), descriptionLength);
        var ranked = i % 2 == 0 ? "true" : "false";
        return $$$"""
                 {"id":"{{{i:x}}}","name":"Song \u00e9{{{i}}}","description":"{{{description}}}",
                 "uploader":{"id":{{{i}}},"name":"mapper{{{i}}}","admin":false,"curator":false},
                 "metadata":{"bpm":174.5,"duration":{{{200 + i}}},"songName":"S{{{i}}}","songSubName":"",
                 "songAuthorName":"A{{{i}}}","levelAuthorName":"M{{{i}}}"},
                 "stats":{"upvotes":{{{i * 3}}},"downvotes":1,"score":0.87,"reviews":2},
                 "uploaded":"2024-03-0{{{i % 9 + 1}}}T12:34:56.789Z","automapper":false,"ranked":{{{ranked}}},
                 "qualified":false,"tags":["Tech","Dance"],"blRanked":false,"blQualified":false,
                 "versions":[{"hash":"{{{i:x8}}}00000000000000000000000000000000","state":"Published",
                 "sageScore":5,"downloadURL":"https://x/{{{i}}}.zip","key":"{{{i:x}}}",
                 "diffs":[{"njs":16.5,"notes":{{{500 + i}}},"bombs":2,"nps":5.25,"characteristic":"Standard",
                 "difficulty":"ExpertPlus","seconds":180.5,"stars":8.25,
                 "paritySummary":{"errors":0,"warns":1,"resets":2}}]}]}
                 """.ReplaceLineEndings(string.Empty);
    }

    /// <summary>
    /// A full cache file. A decoy nested <c>"docs"</c> sits ahead of the real one, so navigation that merely
    /// scans for the property name instead of skipping whole values is caught.
    /// </summary>
    public static byte[] Build(int songCount, int descriptionLength = 30)
    {
        var sb = new StringBuilder();
        sb.Append("""{"info":{"total":1,"docs":["decoy","values"]},"docs":[""");
        for (var i = 0; i < songCount; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(Song(i, descriptionLength));
        }

        sb.Append("""],"date":1717171717}""");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static byte[] Wrap(string docsArrayBody) =>
        Encoding.UTF8.GetBytes("{\"docs\":[" + docsArrayBody + "],\"date\":1}");
}
