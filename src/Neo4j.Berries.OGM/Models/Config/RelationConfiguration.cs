using System.Linq;
using System.Reflection;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Interfaces;

namespace Neo4j.Berries.OGM.Models.Config;

public class RelationConfiguration<TStart, TEnd> : IRelationConfiguration
where TStart : class
where TEnd : class
{
    public string Label { get; }
    public RelationDirection Direction { get; }
    private MergeConfiguration<TEnd> EndNodeMergeConfig { get; } = new MergeConfiguration<TEnd>();
    public IEnumerable<string> EndNodeMergeProperties => EndNodeMergeConfig.IncludedProperties;
    public string[] EndNodeLabels { get; private set; } = [];

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
    public IEnumerable<string> EndNodeMergeProperties => [];
}