using System.Reflection;
using System.Collections;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Utils;


public static class ObjectUtils
{
    internal static object ToNeo4jValue(this object input)
    {
        if (input is Guid || input is Enum)
        {
            return input.ToString();
        }
        return input;
    }
    internal static bool IsDictionary(this object input)
    {
        return input.GetType().IsAssignableTo(typeof(IDictionary));
    }
    internal static Dictionary<string, object> NormalizeValuesForNeo4j(this Dictionary<string, object> input, bool nested = false)
    {
        foreach (var item in input)
        {
            if (item.Value is null && nested)
            {
                input.Remove(item.Key);
                continue;
            }
            else if (item.Value is null) continue;
            if (item.Value.GetType().IsAssignableTo(typeof(IDictionary)))
            {
                input[item.Key] = NormalizeValuesForNeo4j((Dictionary<string, object>)item.Value, true);
                continue;
            }
            else if (item.Value.GetType().IsGenericType)
            {
                input[item.Key] = ((IEnumerable)item.Value).Cast<Dictionary<string, object>>().Select(x => NormalizeValuesForNeo4j(x, true));
                continue;
            }
            if (item.Value is null && nested)
            {
                input.Remove(item.Key);
                continue;
            }
            input[item.Key] = item.Value.ToNeo4jValue();
        }
        return input;
    }
    internal static Dictionary<string, object> ToDictionary(this object node, Dictionary<string, NodeConfiguration> config, IEnumerable<string> mergeProperties = null, int iterations = 0)
    {
        if (iterations > 1)
            return null;
        mergeProperties ??= [];
        var nodeConfig = new NodeConfiguration();
        if (config.TryGetValue(node.GetType().Name, out NodeConfiguration _nodeConfig))
        {
            nodeConfig = _nodeConfig;
        }
        Dictionary<string, object> obj = [];
        foreach (PropertyInfo prop in node.GetType().GetProperties())
        {
            var value = prop.GetValue(node);
            IRelationConfiguration relation = null;
            if (value?.GetType().IsGenericType == true)
            {
                var list = ((IEnumerable)value).Cast<object>().ToList();
                if (!nodeConfig.Relations.TryGetValue(prop.Name, out relation))
                    continue;
                var parsedList = list == null || list.Count == 0 ? null :
                    list
                        .Select(
                            x => x.ToDictionary(config, relation?.EndNodeMergeProperties, iterations + 1)
                        )
                            .Where(x => x != null);
                obj[prop.Name] = parsedList?.Any() == false ? null : parsedList;
                continue;
            }
            if (nodeConfig.Relations.TryGetValue(prop.Name, out relation) && value is not null)
            {
                obj[prop.Name] = value.ToDictionary(config, relation?.EndNodeMergeProperties, iterations + 1);
                continue;
            }
            if (
                (mergeProperties.Any() && mergeProperties.Contains(prop.Name)) ||
                (!mergeProperties.Any() &&
                    ((!nodeConfig.ExcludedProperties.Contains(prop.Name) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                    (nodeConfig.IncludedProperties.Contains(prop.Name) && !nodeConfig.IncludedProperties.IsEmpty)))
                )
                obj[prop.Name] = value.ToNeo4jValue();
            else if (!mergeProperties.Any() && nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty)
            {
                if (value is DateTime)
                {
                    var parsedValue = DateTime.Parse(value.ToString());
                    if (parsedValue == DateTime.MinValue) continue;
                }
                if (value != default)
                    obj[prop.Name] = value.ToNeo4jValue();
            }
        }
        return obj;
    }
}