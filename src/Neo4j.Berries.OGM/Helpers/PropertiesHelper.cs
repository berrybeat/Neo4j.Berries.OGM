using System.Collections;
using System.Reflection;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Helpers;

internal class PropertiesHelper(PropertyInfo[] properties, NodeConfiguration nodeConfig, object source)
{
    public void AddNormalizedParameters(Dictionary<string, object> parameters, string parameterNameFormat, out Dictionary<string, string> safeKeyValueParameters)
    {
        var validNodeProperties = properties.Where(p =>
            (!nodeConfig.ExcludedProperties.Contains(p.Name) && !nodeConfig.ExcludedProperties.IsEmpty) ||
            (nodeConfig.IncludedProperties.Contains(p.Name) && !nodeConfig.IncludedProperties.IsEmpty) ||
            (nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty)
        );
        AddNormalizedParameters(validNodeProperties, parameters, parameterNameFormat, out safeKeyValueParameters);
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