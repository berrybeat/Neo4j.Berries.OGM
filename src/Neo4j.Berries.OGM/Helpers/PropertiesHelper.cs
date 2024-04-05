using System.Collections;
using System.Reflection;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Helpers;

internal class PropertiesHelper(object source)
{
    public IEnumerable<PropertyInfo> GetValidProperties(NodeConfiguration nodeConfig, IRelationConfiguration relationConfig = null)
    {
        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (relationConfig is null)
            return properties.Where(p =>
                (!nodeConfig.ExcludedProperties.Contains(p.Name) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                (nodeConfig.IncludedProperties.Contains(p.Name) && !nodeConfig.IncludedProperties.IsEmpty) ||
                (nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty)
            );
        else
            return properties.Where(p =>
                (relationConfig.EndNodeMergeProperties.Any() && relationConfig.EndNodeMergeProperties.Contains(p.Name)) ||
                (!relationConfig.EndNodeMergeProperties.Any() &&
                    ((!nodeConfig.ExcludedProperties.Contains(p.Name) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                    (nodeConfig.IncludedProperties.Contains(p.Name) && !nodeConfig.IncludedProperties.IsEmpty) ||
                    (nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty))
                )
            ).Where(p => p.GetValue(source) != null);
    }

    public void AddNormalizedParameters(IEnumerable<PropertyInfo> validProperties, Dictionary<string, object> parameters, string parameterNameFormat, out Dictionary<string, string> safeKeyValueParameters)
    {
        safeKeyValueParameters = new Dictionary<string, string>();
        foreach (var prop in validProperties)
        {
            var parameterName = string.Format(parameterNameFormat, parameters.Count);
            var value = prop.GetValue(source);
            parameters.Add(parameterName.Replace("$", ""), value.ToNeo4jValue());
            safeKeyValueParameters.Add(prop.Name, parameterName);
        }
    }

}