using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using Serilog;

namespace BeatSpiderSharp.Extensions;

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

    /// <summary>
    ///     Walks <paramref name="path" /> (e.g. <c>["docs"]</c>) to an array property and yields its elements one at a
    ///     time. <c>JsonSerializer.DeserializeAsyncEnumerable</c> cannot do this - it requires the array to be the
    ///     entire payload.
    /// </summary>
    public static async IAsyncEnumerable<T> DeserializeArrayAsync<T>(Stream stream, JsonTypeInfo<T> typeInfo,
        string[] path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var reader = new Utf8JsonStreamReader(stream);

        // Find the property
        foreach (var fieldName in path)
        {
            if (!await TryAdvanceToPropertyAsync(reader, fieldName, ct))
            {
                throw new System.Text.Json.JsonException("Could not find array property");
            }
        }

        if (reader.TokenType != System.Text.Json.JsonTokenType.StartArray)
        {
            throw new System.Text.Json.JsonException($"Property '{path[^1]}' is not an array");
        }

        while (await reader.ReadAsync(ct))
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.EndArray) yield break;

            var item = await reader.DeserializeAsync(typeInfo, ct);
            if (item != null) yield return item;
        }
    }

    private static async ValueTask<bool> TryAdvanceToPropertyAsync(Utf8JsonStreamReader reader, string propertyName,
        CancellationToken ct)
    {
        // Find the property
        while (await reader.ReadAsync(ct))
        {
            if (reader.TokenType != System.Text.Json.JsonTokenType.PropertyName) continue;

            if (reader.ValueTextEquals(propertyName))
            {
                await reader.ReadAsync(ct);
                return true;
            }

            // Step over the value wholesale, so a nested property of the same name is not mistaken for the target.
            await reader.SkipAsync(ct);
        }

        return false;
    }
}
