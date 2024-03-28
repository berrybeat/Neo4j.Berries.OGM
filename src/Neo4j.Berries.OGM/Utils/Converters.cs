using System.Text.Json;
using System.Text.Json.Serialization;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Utils;

internal static class Converters
{
    private static JsonSerializerOptions _serializerOptions { get; set; }
    public static JsonSerializerOptions SerializerOptions
    {
        get
        {
            if(_serializerOptions is not null) return _serializerOptions;
            _serializerOptions = new JsonSerializerOptions();
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
            return _serializerOptions;
        }
    }
    public static TResult Convert<TResult>(this IRecord record, string key)
    {
        var node = record[key].As<INode>();
        var nodeProperties = JsonSerializer.Serialize(node.Properties);

        return JsonSerializer.Deserialize<TResult>(nodeProperties, SerializerOptions);
    }
}