using Serilog;
using SongDetailsCache;

namespace BeatSpiderSharp.Core;

public abstract class BeatSpider
{
    protected SongDetails SongDetails { get; private set; } = null!;

    protected BeatSpider()
    {
        SetupLogging();
    }

    private void SetupLogging()
    {
        var configuration = new LoggerConfiguration();
        ConfigureLogger(configuration);
        Log.Logger = configuration.CreateLogger();
    }

    protected virtual void ConfigureLogger(LoggerConfiguration configuration)
    {
    }
    
    protected virtual async Task InitAsync()
    {
        Log.Information("BeatSpider initializing");
        SongDetails = await SongDetails.Init();
        Log.Debug("SongDetails initialized");
    }
}