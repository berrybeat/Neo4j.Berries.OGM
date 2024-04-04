using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Interfaces;

public interface IRelationConfiguration
{
    string Label { get; }
    RelationDirection Direction { get; }
    Type EndNodeType { get; }
    IEnumerable<string> EndNodeMergeProperties { get; }
}