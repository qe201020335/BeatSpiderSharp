using Newtonsoft.Json.Linq;

namespace JsonTest;

public static class JsonMapper
{
    private static JObject MapJObj(JObject jObj)
    {
        var map = new JObject();

        foreach (var property in jObj.Properties())
        {
            map.Add(property.Name, MapJToken(property.Value));
        }

        return map;
    }

    private static JArray MapJArray(JArray arr)
    {
        var result = new JArray();
        if (arr.Count == 0)
        {
            return result;
        }
    
        // Assume all elements in the array are of the same type
        result.Add(MapJToken(arr[0]));
        return result;
    }

    public static JToken MapJToken(JToken? token)
    {
        // Console.WriteLine($"{token.Path}: {token.Type}");
        token ??= JValue.CreateNull();

        return token.Type switch
        {
            JTokenType.Array => MapJArray((token as JArray)!),
            JTokenType.Object => MapJObj((token as JObject)!),
            _ => token.Type.ToString()
        };
    }

    private static JObject MergeJObj(JObject obj1, JObject? obj2)
    {
        var map = new JObject();

        var propertyNames = obj1.Properties().Select(p => p.Name);
        var names2 = obj2?.Properties().Select(p => p.Name);
        if (names2 is not null)
        {
            propertyNames = propertyNames.Concat(names2).Distinct();
        }

        foreach (var propertyName in propertyNames)
        {
            var prop1 = obj1[propertyName];
            var prop2 = obj2?[propertyName];

            var result = MergeJToken(prop1, prop2);

            map.Add(propertyName, result);
        }

        return map;
    }

    private static JArray MergeJArray(JArray arr1, JArray? arr2)
    {
        //reduce all elements to one
        var reduced = arr1.Concat(arr2 ?? []).Aggregate<JToken, JToken?>(null, MergeJToken) ?? JValue.CreateNull();
        return [reduced];
    }

    public static JToken MergeJToken(JToken? tok1, JToken? tok2)
    {
        tok1 ??= JValue.CreateNull();
        tok2 ??= JValue.CreateNull();
        if (tok1.Type == JTokenType.Null && tok2.Type == JTokenType.Null)
        {
            return JValue.CreateNull();
        }

        if (tok1.Type != tok2.Type && tok1.Type != JTokenType.Null && tok2.Type != JTokenType.Null)
        {
            throw new ArgumentException(
                $"JToken type mismatch. {nameof(tok1)}: {tok1.Type}, {nameof(tok2)}: {tok2.Type})");
        }

        if (tok1.Type == JTokenType.Null)
        {
            (tok1, tok2) = (tok2, tok1);
        }

        if (tok2.Type == JTokenType.Null)
        {
            tok2 = null;
        }

        return tok1.Type switch
        {
            JTokenType.Array => MergeJArray((tok1 as JArray)!, (JArray?)tok2),
            JTokenType.Object => MergeJObj((tok1 as JObject)!, (JObject?)tok2),
            _ => tok1
        };
    }
}
