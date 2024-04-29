using System.Collections.Concurrent;
using Neo4j.Berries.OGM.Interfaces;

namespace Neo4j.Berries.OGM.Models.Config;

public class NodeConfiguration
{
    public ConcurrentBag<string> IncludedProperties { get; set; } = [];
    public ConcurrentBag<string> ExcludedProperties { get; set; } = [];
    public ConcurrentBag<string> Identifiers { get; set; } = []; 
    public ConcurrentDictionary<string, IRelationConfiguration> Relations { get; set; } = [];
}