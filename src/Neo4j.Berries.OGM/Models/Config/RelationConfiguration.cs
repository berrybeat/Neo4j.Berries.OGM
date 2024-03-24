using Neo4j.Berries.OGM.Enums;

namespace Neo4j.Berries.OGM.Models.Config;

public class RelationConfiguration<TStart, TEnd>(string label, RelationDirection direction) : IRelationConfiguration
where TStart : class
where TEnd : class
{
    public string Label { get; } = label;
    public RelationDirection Direction { get; } = direction;
    public Type EndNodeType { get; } = typeof(TEnd);
    private MergeConfiguration<TEnd> EndNodeMergeConfig { get; } = new MergeConfiguration<TEnd>();
    public IEnumerable<string> EndNodeMergeProperties => EndNodeMergeConfig.IncludedProperties;

    public MergeConfiguration<TEnd> OnMerge()
    {
        return EndNodeMergeConfig;
    }
}