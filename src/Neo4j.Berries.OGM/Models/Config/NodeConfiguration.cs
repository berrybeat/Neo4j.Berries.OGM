using System.Collections.Concurrent;

namespace Neo4j.Berries.OGM.Models.Config;

internal class NodeConfiguration
{
    public ConcurrentBag<string> IncludedProperties { get; set; } = [];
    public ConcurrentBag<string> ExcludedProperties { get; set; } = [];
    public ConcurrentDictionary<string, IRelationConfiguration> Relations { get; set; } = [];
}