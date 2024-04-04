using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Interfaces;

public interface IRelationConfiguration
{
    string Label { get; }
    RelationDirection Direction { get; }
    Type EndNodeType { get; }
    /// <summary>
    /// If this is null, EndNodeType is used. EndNodeLabel is only used for anonymous node configuration.
    /// </summary>
    string EndNodeLabel { get; }
    IEnumerable<string> EndNodeMergeProperties { get; }
}