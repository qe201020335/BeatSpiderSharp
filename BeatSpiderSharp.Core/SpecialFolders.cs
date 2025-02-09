namespace BeatSpiderSharp.Core;

public class SpecialFolders: IDisposable
{
    public string DataFolder { get; }
    
    public string TempFolder { get; }

    public SpecialFolders()
    {
        DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "BeatSpiderSharp");
        TempFolder = Path.Combine(DataFolder, "Temp");

        Directory.CreateDirectory(DataFolder);
        
        if (Directory.Exists(TempFolder))
        {
            Directory.Delete(TempFolder, true);
        }
        
        Directory.CreateDirectory(TempFolder);
    }
    
    ~SpecialFolders()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Directory.Delete(TempFolder, true);
    }
}