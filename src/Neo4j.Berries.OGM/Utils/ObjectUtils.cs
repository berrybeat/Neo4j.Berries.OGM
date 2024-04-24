using System.Collections;
using System.Reflection;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Utils;


public static class ObjectUtils
{
    public static object ToNeo4jValue(this object input)
    {
        if (input is Guid || input is Enum)
        {
            return input.ToString();
        }
        return input;
    }
    public static Dictionary<string, object> ToDictionary(this object node, Dictionary<string, NodeConfiguration> config, IEnumerable<string> mergeProperties = null)
    {
        mergeProperties = mergeProperties ?? [];
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
                obj[prop.Name] = list == null || list.Count == 0
                ? null : list.Select(x => x.ToDictionary(config, relation?.EndNodeMergeProperties));
                continue;
            }
            if (nodeConfig.Relations.TryGetValue(prop.Name, out relation))
            {
                obj[prop.Name] = value?.ToDictionary(config, relation?.EndNodeMergeProperties);
                continue;
            }
            if (
                (mergeProperties.Any() && mergeProperties.Contains(prop.Name)) ||
                (!mergeProperties.Any() &&
                    ((!nodeConfig.ExcludedProperties.Contains(prop.Name) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                    (nodeConfig.IncludedProperties.Contains(prop.Name) && !nodeConfig.IncludedProperties.IsEmpty)))
                )
                obj[prop.Name] = value;
            else if (!mergeProperties.Any() && nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty)
            {
                if (value is DateTime)
                {
                    var parsedValue = DateTime.Parse(value.ToString());
                    if (parsedValue == DateTime.MinValue) continue;
                }
                if (value != default)
                    obj[prop.Name] = value;
            }
        }
        return obj;
    }
}