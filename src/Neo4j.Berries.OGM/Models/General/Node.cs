using System.Text;
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
        var identifiers = props.Where(x => NodeConfig.Identifiers.Contains(x));
        Identifiers.AddRange(props.Where(x => NodeConfig.Identifiers.Contains(x)));
        if (!identifiers.Any() && Neo4jSingletonContext.EnforceIdentifiers)
            throw new InvalidOperationException($"Identifiers are enforced but not provided in the data. Label: {label}");
    }

    private void AppendSingleRelations(IEnumerable<Dictionary<string, object>> nodes)
    {
        var relations = nodes.GetRelations(NodeConfig, x => x.IsDictionary());
        foreach (var relation in relations)
        {
            var node = TryAddRelation(relation.Key, SingleRelations);
            node.Consider([relation.Value as Dictionary<string, object>]);
        }
    }

    private void AppendMultipleRelations(IEnumerable<Dictionary<string, object>> nodes)
    {
        var relations = nodes.GetRelations(NodeConfig, x => x.IsCollection());
        foreach (var relation in relations)
        {
            var node = TryAddRelation(relation.Key, MultipleRelations);
            node.Consider((relation.Value as IEnumerable<Dictionary<string, object>>).ToArray());
        }
    }

    private Node TryAddRelation(string key, Dictionary<string, Node> nodeCollection)
    {
        nodeCollection.TryGetValue(key, out Node node);
        if (node is null)
        {
            var relationConfig = NodeConfig.Relations[key];
            var endNodeLabel = relationConfig.EndNodeLabels[0];
            node = new Node(endNodeLabel, depth + 1);
            nodeCollection.Add(key, node);
        }
        return node;
    }

    public void Create(StringBuilder cypherBuilder, string collection, int nodeSetIndex)
    {
        var alias = ComputeAlias("c", nodeSetIndex, 0);
        var unwindVariable = ComputeAlias("cuv", nodeSetIndex, 0);
        cypherBuilder.AppendLine($"UNWIND {collection} AS {unwindVariable}");
        CreateProperties(alias, unwindVariable, cypherBuilder);
        ProcessRelations(cypherBuilder, nodeSetIndex, alias, unwindVariable, false);
    }

    public void Merge(StringBuilder cypherBuilder, string collection, int nodeSetIndex)
    {
        var alias = ComputeAlias("m", nodeSetIndex, 0);
        var unwindVariable = ComputeAlias("muv", nodeSetIndex, 0);
        cypherBuilder.AppendLine($"UNWIND {collection} AS {unwindVariable}");
        MergeProperties(alias, unwindVariable, cypherBuilder);
        ProcessRelations(cypherBuilder, nodeSetIndex, alias, unwindVariable, true);
    }
    private void ProcessRelations(StringBuilder cypherBuilder, int nodeSetIndex, string alias, string unwindVariable, bool shouldMerge)
    {
        var relationAction = shouldMerge ? "MERGE" : "CREATE";
        foreach (var relation in MultipleRelations)
        {
            var index = MultipleRelations.Keys.ToList().IndexOf(relation.Key);
            var variable = ComputeAlias("muv", nodeSetIndex, index, depth + 1);
            cypherBuilder.AppendLine($"FOREACH ({variable} IN {unwindVariable}.{relation.Key} |");
            var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, variable, nodeSetIndex, index);
            var relationConfig = NodeConfig.Relations[relation.Key];
            cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format()}({targetNodeAlias})");
            cypherBuilder.AppendLine(")");
        }
        foreach (var relation in SingleRelations)
        {
            var index = SingleRelations.Keys.ToList().IndexOf(relation.Key);
            cypherBuilder.AppendLine($"FOREACH (ignored IN CASE WHEN {unwindVariable}.{relation.Key} IS NOT NULL THEN [1] ELSE [] END |");
            var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, $"{unwindVariable}.{relation.Key}", nodeSetIndex, index);
            var relationConfig = NodeConfig.Relations[relation.Key];
            cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format()}({targetNodeAlias})");
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
        foreach (var relation in SingleRelations)
        {
            var relationIndex = SingleRelations.Keys.ToList().IndexOf(relation.Key);
            cypherBuilder.AppendLine($"FOREACH (ignored IN CASE WHEN {variable}.{relation.Key} IS NOT NULL THEN [1] ELSE [] END |");
            var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, $"{variable}.{relation.Key}", nodeSetIndex, relationIndex);
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
        AppendWithSetProperties(cypherBuilder, alias, variable, properties);
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
        AppendWithSetProperties(cypherBuilder, alias, variable, Properties);
    }
    private static void AppendWithSetProperties(StringBuilder cypherBuilder, string alias, string variable, IEnumerable<string> properties)
    {
        if (properties.Any())
        {
            cypherBuilder.Append(" SET ");
            cypherBuilder.Append(string.Join(", ", properties.Select(x => $"{alias}.{x}={variable}.{x}")));
        }
        cypherBuilder.AppendLine();
    }

    private string ComputeAlias(string prefix, int nodeSetIndex, int index, int? _depth = null)
    {
        if ((_depth ?? depth) == 0) return $"{prefix}_{nodeSetIndex}";
        return $"{prefix}_{nodeSetIndex}_{_depth ?? depth}_{index}";
    }
}