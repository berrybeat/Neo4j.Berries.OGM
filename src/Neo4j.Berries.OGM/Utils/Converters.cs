using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Neo4j.Berries.OGM.Utils.CustomConverters;
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
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
            _serializerOptions.Converters.Add(new ZonedDateTimeConverter());
            _serializerOptions.Converters.Add(new LocalDateTimeConverter());
            return _serializerOptions;
        }
    }
    public static TResult Convert<TResult>(this IRecord record, string key)
    {
        var node = record[key].As<IEntity>();
        if (node == null)
        {
            return default(TResult);
        }
        var nodeProperties = JsonSerializer.Serialize(node.Properties, SerializerOptions);

        return JsonSerializer.Deserialize<TResult>(nodeProperties, SerializerOptions);
    }

    public static IList<TResult> ConvertCollection<TResult>(this IRecord record, string key)
    {
        var nodes = record[key].As<IEnumerable<IEntity>>();
        if (nodes == null)
        {
            return default(IList<TResult>);
        }
        IList<TResult> result = new List<TResult>();
        foreach (var node in nodes)
        {
            var nodeProperties = JsonSerializer.Serialize(node.Properties, SerializerOptions);

            var n = JsonSerializer.Deserialize<TResult>(nodeProperties, SerializerOptions);

            result.Add(n);
        }
        return result;
    }
}