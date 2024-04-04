using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Models.Config;

public class NodeConfigurationBuilder
{
    internal NodeConfiguration NodeConfiguration { get; } = new NodeConfiguration();
    /// <summary>
    /// Includes the given properties for mapping in NodeConfiguration
    /// </summary>
    /// <param name="properties">The properties to include for mapping</param>
    /// <exception cref="InvalidOperationException">If the property is already excluded</exception>
    public NodeConfigurationBuilder IncludeProperties(params string[] properties)
    {
        foreach (var prop in properties)
        {
            if (NodeConfiguration.ExcludedProperties.Contains(prop))
            {
                throw new InvalidOperationException($"Property '{prop}' is already excluded.");
            }
            NodeConfiguration.IncludedProperties.Add(prop);
        }
        return this;
    }
    /// <summary>
    /// Excludes the given properties from mapping in NodeConfiguration
    /// </summary>
    /// <param name="properties">The properties to exclude for mapping</param>
    /// <exception cref="InvalidOperationException">If the property is already included</exception>
    public NodeConfigurationBuilder ExcludeProperties(params string[] properties)
    {
        foreach (var prop in properties)
        {
            if (NodeConfiguration.IncludedProperties.Contains(prop))
            {
                throw new InvalidOperationException($"Property '{prop}' is already included.");
            }
            NodeConfiguration.ExcludedProperties.Add(prop);
        }
        return this;
    }

    /// <summary>
    /// Adds a relation configuration to the NodeConfiguration where the property name is the same as the target node label
    /// </summary>
    /// <param name="TargetNodeLabel">The label of the target node</param>
    /// <param name="RelationLabel">The label of the relation</param>
    /// <param name="Direction">The direction of the relation</param>
    public NodeConfigurationBuilder HasRelation(string TargetNodeLabel, string RelationLabel, RelationDirection Direction)
    {
        this.HasRelation(TargetNodeLabel, TargetNodeLabel, RelationLabel, Direction);
        return this;
    }
    /// <summary>
    /// Adds a relation configuration to the NodeConfiguration
    /// </summary>
    /// <param name="Property">The property name which this configuration is for</param>
    /// <param name="TargetNodeLabel">The label of the target node</param>
    /// <param name="RelationLabel">The label of the relation</param>
    /// <param name="Direction">The direction of the relation</param>
    /// <exception cref="InvalidOperationException">If the property is already included</exception>
    public NodeConfigurationBuilder HasRelation(string Property, string TargetNodeLabel, string RelationLabel, RelationDirection Direction)
    {
        NodeConfiguration.Relations[Property] = new RelationConfiguration(TargetNodeLabel, RelationLabel, Direction);
        ExcludeProperties(Property);
        return this;
    }
}