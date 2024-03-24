using berrybeat.Neo4j.OGM.Enums;

internal interface IRelationConfiguration
{
    string Label { get; }
    RelationDirection Direction { get; }
    Type EndNodeType { get; }
    IEnumerable<string> EndNodeMergeProperties { get; }
}