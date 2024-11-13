using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;

namespace Neo4j.Berries.OGM.Models.Config;

public class RelationConfiguration<TStart, TEnd> : IRelationConfiguration
where TStart : class
where TEnd : class
{
    /// <summary>
    /// The label of the relation
    /// </summary>
    public string Label { get; }
    /// <summary>
    /// The direction of the relation
    /// </summary>
    public RelationDirection Direction { get; }
    /// <summary>
    /// If there is merge with the end node, it can be configured how the merge should be done
    /// </summary>
    private MergeConfiguration<TEnd> EndNodeMergeConfig { get; } = new MergeConfiguration<TEnd>();
    public IEnumerable<string> EndNodeMergeProperties => EndNodeMergeConfig.IncludedProperties;
    /// <summary>
    /// The labels of the end nodes
    /// </summary>
    public string[] EndNodeLabels { get; private set; } = [];
    /// <summary>
    /// If true, the history of the relation will be kept. It will set all the non archived relations to archived.
    /// </summary>
    public bool KeepHistory { get; set; } = false;

    /// <summary>
    /// The name of the relation property on the start node
    /// </summary>
    public string Property { get; internal set; }
    /// <summary>
    /// Indicates whether the relation property is a collection
    /// </summary>
    public bool IsCollection { get; internal set; } = false;

    public RelationConfiguration(string label, RelationDirection direction)
    {
        Label = label;
        Direction = direction;
        ProcessEndNodeLabels();
    }

    private void ProcessEndNodeLabels()
    {
        var endNodeType = typeof(TEnd);
        if (!endNodeType.IsInterface)
        {
            EndNodeLabels = [.. EndNodeLabels, endNodeType.Name];
            return;
        }
        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => !x.IsInterface)
            .Where(x => x.GetInterfaces().Contains(endNodeType));
        EndNodeLabels = implementations.Select(x => x.Name).ToArray();
    }

    public MergeConfiguration<TEnd> OnMerge()
    {
        return EndNodeMergeConfig;
    }
}

public class RelationConfiguration(string[] endNodeLabels, string label, RelationDirection direction) : IRelationConfiguration
{
    public string Label => label;
    public RelationDirection Direction => direction;
    public string[] EndNodeLabels => endNodeLabels;
    /// <summary>
    /// For the anonymous relations, this option is not available.
    /// </summary>
    public IEnumerable<string> EndNodeMergeProperties => [];

    public string Property { get; internal set; }
    public bool IsCollection { get; internal set; } = false;

    /// <summary>
    /// If true, the history of the relation will be kept. It will set all the non archived relations to archived.
    /// </summary>
    public bool KeepHistory { get; set; } = false;
}