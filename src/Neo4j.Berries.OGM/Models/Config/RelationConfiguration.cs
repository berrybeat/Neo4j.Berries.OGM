using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;

namespace Neo4j.Berries.OGM.Models.Config;

public class RelationConfiguration<TStart, TEnd>(string label, RelationDirection direction) : IRelationConfiguration
where TStart : class
where TEnd : class
{
    public string Label { get; } = label;
    public RelationDirection Direction { get; } = direction;
    private MergeConfiguration<TEnd> EndNodeMergeConfig { get; } = new MergeConfiguration<TEnd>();
    public IEnumerable<string> EndNodeMergeProperties => EndNodeMergeConfig.IncludedProperties;
    public string EndNodeLabel { get; } = typeof(TEnd).Name;

    public MergeConfiguration<TEnd> OnMerge()
    {
        return EndNodeMergeConfig;
    }
}

public class RelationConfiguration(string endNodeLabel, string label, RelationDirection direction) : IRelationConfiguration
{
    public string Label => label;
    public RelationDirection Direction => direction;
    public string EndNodeLabel => endNodeLabel;
    public IEnumerable<string> EndNodeMergeProperties => [];
}