using System.Collections;
using System.Reflection;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Helpers;

internal class PropertiesHelper(object source)
{
    public Dictionary<string, object> GetValidProperties(NodeConfiguration nodeConfig, IRelationConfiguration relationConfig = null)
    {
        Dictionary<string, object> properties = [];
        if (source is not Dictionary<string, object>)
        {
            properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p.GetValue(source));
        }
        else
        {
            properties = source as Dictionary<string, object>;
        }

        if (relationConfig is null)
            return properties.Where(p =>
                (!nodeConfig.ExcludedProperties.Contains(p.Key) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                (nodeConfig.IncludedProperties.Contains(p.Key) && !nodeConfig.IncludedProperties.IsEmpty) ||
                (nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty)
            )
            .ToDictionary(p => p.Key, p => p.Value);
        else
            return properties.Where(p =>
                (relationConfig.EndNodeMergeProperties.Any() && relationConfig.EndNodeMergeProperties.Contains(p.Key)) ||
                (!relationConfig.EndNodeMergeProperties.Any() &&
                    ((!nodeConfig.ExcludedProperties.Contains(p.Key) && !nodeConfig.ExcludedProperties.IsEmpty) ||
                    (nodeConfig.IncludedProperties.Contains(p.Key) && !nodeConfig.IncludedProperties.IsEmpty) ||
                    (nodeConfig.ExcludedProperties.IsEmpty && nodeConfig.IncludedProperties.IsEmpty))
                )
            ).Where(p => p.Value != null)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}