using System.Text;
using Newtonsoft.Json;
using Serilog;

namespace BeatSpiderSharp.Core.Utilities.Extensions;

public static class JsonExtensions
{
    public static T? DeserializeObject<T>(this JsonSerializer serializer, string path) where T : class
    {
        if (!File.Exists(path))
        {
            Log.Error("File {Path} does not exist", path);
            return null;
        }

        Log.Debug("Deserializing {Type} from {Path}", typeof(T).Name, path);
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var jsonReader = new JsonTextReader(reader);
        return serializer.Deserialize<T>(jsonReader);
    }
    
    public static void Serialize(this JsonSerializer serializer, object? value, string path)
    {
        Log.Debug("Serializing {Type} to {Path}", value?.GetType().Name, path);
        using var outputStream = new FileStream(path, FileMode.Create);
        using var textWriter = new StreamWriter(outputStream, Encoding.UTF8);
        using var jsonWriter = new JsonTextWriter(textWriter);
        serializer.Serialize(jsonWriter, value);
    }
}