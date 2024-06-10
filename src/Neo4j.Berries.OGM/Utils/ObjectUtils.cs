using System.Reflection;
using System.Collections;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Microsoft.VisualBasic;

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
    private static bool IsListOfInterfaces(this List<object> inputType)
    {
        var interfaces = inputType
            .SelectMany(x => x.GetType().GetInterfaces())
            .Distinct();
        return interfaces
            .Any(
                x =>
                    inputType
                        .GetType()
                        .GetGenericArguments()
                        .Any(y => y.IsAssignableFrom(x))
                );
    }
    /// <summary>
    /// Checks if the object is a collection, but being a dictionary collection is excluded.
    /// </summary>
    internal static bool IsCollection(this object input)
    {
        return input.GetType().IsAssignableTo(typeof(IEnumerable)) && !input.IsDictionary();
    }
    internal static Dictionary<string, object> NormalizeValuesForNeo4j(this Dictionary<string, object> input, bool recursion = false)
    {
        foreach (var item in input)
        {
            if (item.Value is null && recursion)
            {
                input.Remove(item.Key);
                continue;
            }
            else if (item.Value is null) continue;
            if (item.Value.GetType().IsAssignableTo(typeof(IDictionary)))
            {
                var dict = (Dictionary<string, object>)item.Value;
                if (dict.Keys.Count == 0)
                {
                    input[item.Key] = null;
                    continue;
                }
                input[item.Key] = NormalizeValuesForNeo4j(dict, true);
                continue;
            }
            else if (item.Value.GetType().IsGenericType)
            {
                var genericArgumentType = item.Value.GetType().GetGenericArguments().First();
                if (genericArgumentType.IsAssignableTo(typeof(IDictionary)))
                {
                    input[item.Key] = ((IEnumerable)item.Value).Cast<Dictionary<string, object>>().Select(x => NormalizeValuesForNeo4j(x, true));
                    continue;
                }
            }
            if (item.Value is null && recursion)
            {
                input.Remove(item.Key);
                continue;
            }
            input[item.Key] = item.Value.ToNeo4jValue();
        }
        return input;
    }
    internal static Dictionary<string, object> ToDictionary(this object node, Dictionary<string, NodeConfiguration> config, Func<string, string> propertyCaseConverter = null, IEnumerable<string> mergeProperties = null, int iterations = 0)
    {
        if (iterations > 1)
            return null;
        mergeProperties ??= [];
        var nodeConfig = new NodeConfiguration();
        if (config.TryGetValue(node.GetType().Name, out NodeConfiguration _nodeConfig))
        {
            nodeConfig = _nodeConfig;
        }
        propertyCaseConverter ??= ((x) => x);
        Dictionary<string, object> obj = [];
        foreach (PropertyInfo prop in node.GetType().GetProperties())
        {
            var propName = propertyCaseConverter(prop.Name);
            var value = prop.GetValue(node);
            IRelationConfiguration relation = null;
            if (value?.GetType().IsGenericType == true)
            {
                var list = ((IEnumerable)value).Cast<object>().ToList();
                if (!nodeConfig.Relations.TryGetValue(prop.Name, out relation))
                    continue;
                if (list.IsListOfInterfaces())
                {
                    obj[prop.Name] = new Dictionary<string, List<Dictionary<string, object>>>();
                    foreach (var item in list)
                    {
                        var record = obj[prop.Name] as Dictionary<string, List<Dictionary<string, object>>>;
                        var typeName = item.GetType().Name;
                        if (!record.ContainsKey(item.GetType().Name))
                        {
                            record[typeName] = [];
                        }
                        record[typeName]
                            .Add(item.ToDictionary(
                                config, propertyCaseConverter, relation?.EndNodeMergeProperties, iterations + 1));
                    }
                }
                else
                {
                    var parsedList = list == null || list.Count == 0 ? null :
                        list
                            .Select(
                                x => x.ToDictionary(config, propertyCaseConverter, relation?.EndNodeMergeProperties, iterations + 1)
                            ).Where(x => x != null);
                    obj[prop.Name] = parsedList?.Any() == false ? null : parsedList;
                }
                continue;
            }
            if (nodeConfig.Relations.TryGetValue(prop.Name, out relation) && value is not null)
            {
                obj[prop.Name] = value.ToDictionary(config, propertyCaseConverter, relation?.EndNodeMergeProperties, iterations + 1);
                continue;
            }
            if (
                (mergeProperties.Any() && mergeProperties.Contains(propName)) ||
                (!mergeProperties.Any() &&
                    ((!nodeConfig.ExcludedProperties.Contains(propName) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                    (nodeConfig.IncludedProperties.Contains(propName) && !nodeConfig.IncludedProperties.IsEmpty)))
                )
                obj[propName] = value.ToNeo4jValue();
            else if (!mergeProperties.Any() && nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty)
            {
                if (value is DateTime)
                {
                    var parsedValue = DateTime.Parse(value.ToString());
                    if (parsedValue == DateTime.MinValue) continue;
                }
                if (value != default)
                    obj[propName] = value.ToNeo4jValue();
            }
        }
        return obj;
    }
}