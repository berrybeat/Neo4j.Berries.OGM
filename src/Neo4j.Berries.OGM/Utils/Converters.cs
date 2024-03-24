using System.Text.Json;
using Neo4j.Driver;

namespace Neo4j.Berries.OGM.Utils;

internal static class Converters
{
    public static TResult Convert<TResult>(this IRecord record, string key)
    {
        var node = record[key].As<INode>();
        var nodeProperties = JsonSerializer.Serialize(node.Properties);
        return JsonSerializer.Deserialize<TResult>(nodeProperties);
    }
}