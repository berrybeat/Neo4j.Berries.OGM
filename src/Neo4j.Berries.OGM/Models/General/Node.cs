using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

internal class Node(string label, int depth = 0)
{
    public List<string> Identifiers { get; set; } = [];
    public List<string> Properties { get; set; } = []; //These should be merged, and if there is a parent, it will merge a relation too.
    public Dictionary<string, Node> SingleRelations { get; set; } = [];
    public Dictionary<string, Node> MultipleRelations { get; set; } = [];
    public NodeConfiguration NodeConfig
    {
        get
        {
            Neo4jSingletonContext.Configs.TryGetValue(label, out NodeConfiguration nodeConfig);
            return nodeConfig ?? new();
        }
    }
    public Node Consider(IEnumerable<Dictionary<string, object>> nodes)
    {
        AppendProperties(nodes);
        AppendSingleRelations(nodes);
        AppendMultipleRelations(nodes);
        return this;
    }

    private void AppendProperties(IEnumerable<Dictionary<string, object>> nodes)
    {
        var props = nodes
            .SelectMany(x => x)
            .Where(x => x.Value != null)
            .Where(x => !NodeConfig.Relations.ContainsKey(x.Key))
            .Select(x => x.Key)
            .Where(x => !Identifiers.Contains(x) && !Properties.Contains(x))
            .Distinct();
        Properties.AddRange(props.Where(x => !NodeConfig.Identifiers.Contains(x)));
        Identifiers.AddRange(props.Where(x => NodeConfig.Identifiers.Contains(x)));
    }

    private void AppendSingleRelations(IEnumerable<Dictionary<string, object>> nodes)
    {
        var relations = nodes
            .SelectMany(x => x)
            .Where(x => x.Value != null)
            .Where(x => NodeConfig.Relations.ContainsKey(x.Key))
            .Where(x => x.Value.IsDictionary());
        foreach (var relation in relations)
        {
            SingleRelations.TryGetValue(relation.Key, out Node node);
            if (node is null)
            {
                var relationConfig = NodeConfig.Relations[relation.Key];
                var endNodeLabel = relationConfig.EndNodeLabel ?? relationConfig.EndNodeType.Name;
                node = new Node(endNodeLabel, depth + 1);
                SingleRelations.Add(relation.Key, node);
            }
            node.Consider([relation.Value as Dictionary<string, object>]);
        }
    }

    private void AppendMultipleRelations(IEnumerable<Dictionary<string, object>> nodes)
    {
        var relations = nodes
            .SelectMany(x => x)
            .Where(x => x.Value != null)
            .Where(x => NodeConfig.Relations.ContainsKey(x.Key))
            .Where(x => x.Value.IsCollection());
        foreach (var relation in relations)
        {
            MultipleRelations.TryGetValue(relation.Key, out Node node);
            if (node is null)
            {
                var relationConfig = NodeConfig.Relations[relation.Key];
                var endNodeLabel = relationConfig.EndNodeLabel ?? relationConfig.EndNodeType.Name;
                node = new Node(endNodeLabel, depth + 1);
                MultipleRelations.Add(relation.Key, node);
            }
            node.Consider((relation.Value as IEnumerable<Dictionary<string, object>>).ToArray());
        }
    }
    public void Create(StringBuilder cypherBuilder, string collection, int nodeSetIndex)
    {
        var alias = ComputeAlias("c", nodeSetIndex, 0);
        var unwindVariable = ComputeAlias("cuv", nodeSetIndex, 0);
        cypherBuilder.AppendLine($"UNWIND {collection} AS {unwindVariable}");
        CreateProperties(alias, unwindVariable, cypherBuilder);
        foreach (var relation in MultipleRelations)
        {
            var index = MultipleRelations.Keys.ToList().IndexOf(relation.Key);
            var variable = ComputeAlias("muv", nodeSetIndex, index, depth + 1);
            cypherBuilder.AppendLine($"FOREACH ({variable} IN {unwindVariable}.{relation.Key} |");
            var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, variable, nodeSetIndex, index);
            var relationConfig = NodeConfig.Relations[relation.Key];
            cypherBuilder.AppendLine($"CREATE ({alias}){relationConfig.Format()}({targetNodeAlias})");
            cypherBuilder.AppendLine(")");
        }
    }

    public void Merge(StringBuilder cypherBuilder, string collection, int nodeSetIndex)
    {
        var alias = ComputeAlias("m", nodeSetIndex, 0);
        var unwindVariable = ComputeAlias("muv", nodeSetIndex, 0);
        cypherBuilder.AppendLine($"UNWIND {collection} AS {unwindVariable}");
        MergeProperties(alias, unwindVariable, cypherBuilder);
        foreach (var relation in MultipleRelations)
        {
            var index = MultipleRelations.Keys.ToList().IndexOf(relation.Key);
            var variable = ComputeAlias("muv", nodeSetIndex, index, depth + 1);
            cypherBuilder.AppendLine($"FOREACH ({variable} IN {unwindVariable}.{relation.Key} |");
            var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, variable, nodeSetIndex, index);
            var relationConfig = NodeConfig.Relations[relation.Key];
            cypherBuilder.AppendLine($"MERGE ({alias}){relationConfig.Format()}({targetNodeAlias})");
            cypherBuilder.AppendLine(")");
        }
    }
    public string MergeRelations(StringBuilder cypherBuilder, string variable, int nodeSetIndex, int index)
    {
        var alias = ComputeAlias("m", nodeSetIndex, index);
        MergeProperties(alias, variable, cypherBuilder);
        foreach (var relation in MultipleRelations)
        {
            var relationIndex = MultipleRelations.Keys.ToList().IndexOf(relation.Key);
            var nextDepthVariable = ComputeAlias("muv", nodeSetIndex, relationIndex, depth + 1);
            cypherBuilder.AppendLine($"FOREACH ({nextDepthVariable} IN {variable}.{relation.Key} |");
            var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, nextDepthVariable, nodeSetIndex, relationIndex);
            var relationConfig = NodeConfig.Relations[relation.Key];
            cypherBuilder.AppendLine($"MERGE ({alias}){relationConfig.Format()}({targetNodeAlias})");
            cypherBuilder.AppendLine(")");
        }
        return alias;
    }

    private void CreateProperties(string alias, string variable, StringBuilder cypherBuilder)
    {
        cypherBuilder.Append($"CREATE ({alias}:{label})");
        var properties = Identifiers.Concat(Properties);
        cypherBuilder.Append(" SET ");
        cypherBuilder.Append(string.Join(", ", properties.Select(x => $"{alias}.{x}={variable}.{x}")));
        cypherBuilder.AppendLine();
    }
    private void MergeProperties(string alias, string variable, StringBuilder cypherBuilder)
    {
        cypherBuilder.Append($"MERGE ({alias}:{label}");
        if (Identifiers.Count > 0)
        {
            cypherBuilder.Append(" {");
            cypherBuilder.Append(string.Join(", ", Identifiers.Select(x => $"{x}: {variable}.{x}")));
            cypherBuilder.Append('}');
        }
        cypherBuilder.Append(')');
        if (Properties.Count > 0)
        {
            cypherBuilder.Append(" SET ");
            cypherBuilder.Append(string.Join(", ", Properties.Select(x => $"{alias}.{x}={variable}.{x}")));
        }
        cypherBuilder.AppendLine();
    }

    private string ComputeAlias(string prefix, int nodeSetIndex, int index, int? _depth = null)
    {
        if ((_depth ?? depth) == 0) return $"{prefix}_{nodeSetIndex}";
        return $"{prefix}_{nodeSetIndex}_{_depth ?? depth}_{index}";
    }
}