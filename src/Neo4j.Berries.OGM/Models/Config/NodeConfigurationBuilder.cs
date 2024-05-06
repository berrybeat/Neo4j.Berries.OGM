using System.ComponentModel;
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
    /// <param name="targetNodeLabel">The labels of the target nodes</param>
    /// <param name="relationLabel">The label of the relation</param>
    /// <param name="direction">The direction of the relation</param>
    public NodeConfigurationBuilder HasRelation(string targetNodeLabel, string relationLabel, RelationDirection direction)
    {
        HasRelation([targetNodeLabel], relationLabel, direction);
        return this;
    }

    /// <summary>
    /// Adds a relation configuration to the NodeConfiguration where the property name is the same as the target node label
    /// </summary>
    /// <param name="targetNodeLabels">The labels of the target nodes</param>
    /// <param name="relationLabel">The label of the relation</param>
    /// <param name="direction">The direction of the relation</param>
    public NodeConfigurationBuilder HasRelation(string[] targetNodeLabels, string relationLabel, RelationDirection direction)
    {
        HasRelation(targetNodeLabels[0], targetNodeLabels, relationLabel, direction);
        return this;
    }

    /// <summary>
    /// Adds a relation configuration to the NodeConfiguration
    /// </summary>
    /// <param name="property">The property name which this configuration is for</param>
    /// <param name="targetNodeLabel">The labels of the target nodes</param>
    /// <param name="relationLabel">The label of the relation</param>
    /// <param name="direction">The direction of the relation</param>
    /// <exception cref="InvalidOperationException">If the property is already included</exception>
    public NodeConfigurationBuilder HasRelation(string property, string targetNodeLabel, string relationLabel, RelationDirection direction)
    {
        HasRelation(property, [targetNodeLabel], relationLabel, direction);
        return this;
    }

    /// <summary>
    /// Adds a relation configuration to the NodeConfiguration
    /// </summary>
    /// <param name="property">The property name which this configuration is for</param>
    /// <param name="targetNodeLabels">The labels of the target nodes</param>
    /// <param name="relationLabel">The label of the relation</param>
    /// <param name="direction">The direction of the relation</param>
    /// <exception cref="InvalidOperationException">If the property is already included</exception>
    public NodeConfigurationBuilder HasRelation(string property, string[] targetNodeLabels, string relationLabel, RelationDirection direction)
    {
        NodeConfiguration.Relations[property] = new RelationConfiguration(targetNodeLabels, relationLabel, direction);
        ExcludeProperties(property);
        return this;
    }

    /// <summary>
    /// The property will be used as an identifier for the node.
    /// </summary>
    /// <param name="Property">The property to use as an identifier</param>
    /// <remarks>
    /// The identifier is used to find the node in the database and the value for the identifier must not be null.
    /// </remarks>
    public NodeConfigurationBuilder HasIdentifier(string Property)
    {
        NodeConfiguration.Identifiers.Add(Property);
        return this;
    }
    /// <summary>
    /// The properties will be used as identifiers for the node.
    /// </summary>
    /// <param name="properties">The properties to use as identifiers</param>
    /// <remarks>
    /// The identifiers is used to find the node in the database and the value for the identifier must not be null.
    /// </remarks>

    public NodeConfigurationBuilder HasIdentifiers(params string[] properties)
    {
        properties.ToList().ForEach(x => HasIdentifier(x));
        return this;
    }
}