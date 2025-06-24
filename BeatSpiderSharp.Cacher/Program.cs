using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text.Json;

Console.ForegroundColor = ConsoleColor.Green;
var progress = new Progress<ProgressReport>(report =>
{
    if (report.Error != null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"抓取页面 {report.CurrentPage} 时发生错误: {report.Error.Message}");
        Console.ForegroundColor = ConsoleColor.Green;
    }
    else
    {
        Console.WriteLine($"已获取 {report.CurrentPage}/{report.TotalPages} 页 ({Math.Round((double)report.CurrentPage / report.TotalPages * 100, 2)}%)");
    }
});

using var crawler = new BeatsaverCrawler(progress);
await crawler.CrawlAllMapsAsync("localcache.saver");
Console.WriteLine("完整本地缓存已保存到 localcache.saver");

public class ProgressReport
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public Exception Error { get; set; }
}

public class BeatsaverCrawler : IDisposable
{
    private const string ApiUrl = "https://api.beatsaver.com/search/text/";
    private const int PageSize = 100;
    private readonly HttpClient _client;
    private readonly IProgress<ProgressReport> _progress;
    private readonly ConcurrentBag<string> _tempFiles = new();

    public BeatsaverCrawler(IProgress<ProgressReport> progress)
    {
        _progress = progress;
        _client = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 5
        });
    }

    public async Task CrawlAllMapsAsync(string outputPath)
    {
        var totalPages = await GetTotalPagesAsync();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
        var writerLock = new object();
        using (var outputStream = new FileStream("localcache.saver", FileMode.Create))
        using (var writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions
        {
            Indented = false
        }))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("docs");
            writer.WriteStartArray();
            await Parallel.ForEachAsync(Enumerable.Range(0, totalPages), options, async (page, ct) =>
            {
                using var inputStream = await ProcessPageAsync(page, totalPages);
                var doc = JsonDocument.Parse(inputStream);
                if (doc.RootElement.TryGetProperty("docs", out var docsArray))
                {
                    lock (writerLock)
                    {
                        foreach (var item in docsArray.EnumerateArray())
                        {
                            item.WriteTo(writer);
                        }
                    }
                }
            });
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    private async Task<int> GetTotalPagesAsync()
    {
        var response = await _client.GetStringAsync($"{ApiUrl}0?pageSize={PageSize}");
        var doc = JsonDocument.Parse(response);
        return (int)Math.Ceiling(doc.RootElement.GetProperty("info").GetProperty("total").GetInt32() / (double)PageSize);
    }

    private async Task<Stream> ProcessPageAsync(int page, int totalPages)
    {
        try
        {
            var response = await _client.GetStringAsync($"{ApiUrl}{page}?pageSize={PageSize}");
            _progress?.Report(new ProgressReport
            {
                CurrentPage = page + 1,
                TotalPages = totalPages
            });
            var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
            return memoryStream;
        }
        catch (Exception ex)
        {
            _progress?.Report(new ProgressReport { Error = ex });
            return Stream.Null;
        }
    }

    public void Dispose() => _client.Dispose();
}