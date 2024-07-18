using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models.Sets;

internal class Node(string label, int depth = 0)
{
    public List<string> Identifiers { get; set; } = [];
    public List<string> Properties { get; set; } = []; //These should be merged. If there is a parent, it will merge a relation too.
    public Dictionary<string, Node> SingleRelations { get; set; } = [];
    public Dictionary<string, Node> MultipleRelations { get; set; } = [];
    public Dictionary<string, Dictionary<string, Node>> GroupRelations { get; set; } = [];

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
        AppendGroupRelations(nodes);
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
            .Where(x => !Properties.Contains(x.Key))
            .Select(x => x.Key);
        Properties.AddRange(props.Distinct().Where(x => !NodeConfig.Identifiers.Contains(x)));
        var identifiers = props.Where(x => NodeConfig.Identifiers.Contains(x));
        Identifiers.AddRange(props.Distinct().Where(x => NodeConfig.Identifiers.Contains(x)));
        if (identifiers.Count() != nodes.Count() && Neo4jSingletonContext.EnforceIdentifiers)
            throw new InvalidOperationException($"Identifiers are enforced but not provided in the data. Label: {label}");
    }

    public void AppendGroupRelations(IEnumerable<Dictionary<string, object>> nodes)
    {
        var relations = nodes
            .GetRelations(NodeConfig, x => x.IsDictionary())
            .Where(x => NodeConfig.Relations[x.Key].EndNodeLabels.Length > 1);
        if (relations.Any(x => (x.Value as Dictionary<string, object>).Any(y => y.Value.IsDictionary())))
        {
            throw new InvalidOperationException("Group items should be collections.");
        }
        foreach (var relation in relations)
        {
            var value = relation.Value as Dictionary<string, object>;
            GroupRelations.TryGetValue(relation.Key, out Dictionary<string, Node> nodeCollection);
            if (nodeCollection is null)
            {
                nodeCollection = [];
                GroupRelations.Add(relation.Key, nodeCollection);
            }
            foreach (var member in value)
            {
                var node = TryAddRelation(relation.Key, nodeCollection, member.Key);
                node.Consider((member.Value as IEnumerable<Dictionary<string, object>>).ToArray());
            }
        }
    }

    private void AppendSingleRelations(IEnumerable<Dictionary<string, object>> nodes)
    {
        var relations = nodes
            .GetRelations(NodeConfig, x => x.IsDictionary())
            .Where(x => !GroupRelations.ContainsKey(x.Key));
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
        return TryAddRelation(key, nodeCollection, null);
    }

    private Node TryAddRelation(string key, Dictionary<string, Node> nodeCollection, string memberKey)
    {
        nodeCollection.TryGetValue(memberKey ?? key, out Node node);
        if (node is null)
        {
            var relationConfig = NodeConfig.Relations[key];
            string endNodeLabel = relationConfig.EndNodeLabels[0];
            if (!string.IsNullOrEmpty(memberKey))
            {
                endNodeLabel = relationConfig.EndNodeLabels.First(x => x.Equals(memberKey));
            }
            node = new Node(endNodeLabel, depth + 1);
            nodeCollection.Add(memberKey ?? key, node);
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
            AppendMultipleRelationCypher(cypherBuilder, relation, unwindVariable, alias, nodeSetIndex, relationAction);
        }
        foreach (var relation in SingleRelations)
        {
            AppendSingleRelationCypher(cypherBuilder, relation, unwindVariable, alias, nodeSetIndex, relationAction);
        }
        foreach (var relation in GroupRelations)
        {
            AppendGroupRelationCypher(cypherBuilder, relation, unwindVariable, alias, nodeSetIndex, relationAction);
        }
    }
    public string MergeRelations(StringBuilder cypherBuilder, string variable, int nodeSetIndex, int index)
    {
        var alias = ComputeAlias("m", nodeSetIndex, index);
        MergeProperties(alias, variable, cypherBuilder);
        foreach (var relation in MultipleRelations)
        {
            AppendMultipleRelationCypher(cypherBuilder, relation, variable, alias, nodeSetIndex);
        }
        foreach (var relation in SingleRelations)
        {
            AppendSingleRelationCypher(cypherBuilder, relation, variable, alias, nodeSetIndex);
        }
        foreach (var relation in GroupRelations)
        {
            AppendGroupRelationCypher(cypherBuilder, relation, variable, alias, nodeSetIndex);
        }
        return alias;
    }

    private void AppendSingleRelationCypher(StringBuilder cypherBuilder, KeyValuePair<string, Node> relation, string variable, string alias, int nodeSetIndex, string relationAction = "MERGE")
    {
        var relationIndex = SingleRelations.Keys.ToList().IndexOf(relation.Key);
        cypherBuilder.AppendLine($"FOREACH (ignored IN CASE WHEN {variable}.{relation.Key} IS NOT NULL THEN [1] ELSE [] END |");
        var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, $"{variable}.{relation.Key}", nodeSetIndex, relationIndex);
        var relationConfig = NodeConfig.Relations[relation.Key];
        var timestampConfig = Neo4jSingletonContext.TimestampConfiguration;
        if (timestampConfig.Enabled)
        {
            var relationAlias = ComputeAlias("r", nodeSetIndex, relationIndex);
            cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format(relationAlias)}({targetNodeAlias})");
            if (relationAction == "MERGE")
            {
                if (timestampConfig.EnforceModifiedTimestampKey)
                    cypherBuilder.AppendLine($"ON CREATE SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp(), {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                else
                    cypherBuilder.AppendLine($"ON CREATE SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                cypherBuilder.AppendLine($"ON MATCH SET {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
            }
            else
            {
                cypherBuilder.Append($"SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                if (timestampConfig.EnforceModifiedTimestampKey)
                    cypherBuilder.Append($", {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                cypherBuilder.AppendLine();
            }
        }
        else
        {
            cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format()}({targetNodeAlias})");
        }
        cypherBuilder.AppendLine(")");
    }

    private void AppendMultipleRelationCypher(StringBuilder cypherBuilder, KeyValuePair<string, Node> relation, string variable, string alias, int nodeSetIndex, string relationAction = "MERGE")
    {
        var index = MultipleRelations.Keys.ToList().IndexOf(relation.Key);
        var nextDepthVariable = ComputeAlias("muv", nodeSetIndex, index, depth + 1);
        cypherBuilder.AppendLine($"FOREACH ({nextDepthVariable} IN {variable}.{relation.Key} |");
        var targetNodeAlias = relation.Value.MergeRelations(cypherBuilder, nextDepthVariable, nodeSetIndex, index);
        var relationConfig = NodeConfig.Relations[relation.Key];
        var timestampConfig = Neo4jSingletonContext.TimestampConfiguration;
        if (timestampConfig.Enabled)
        {
            var relationAlias = ComputeAlias("r", nodeSetIndex, index);
            cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format(relationAlias)}({targetNodeAlias})");
            if (relationAction == "MERGE")
            {
                if (timestampConfig.EnforceModifiedTimestampKey)
                    cypherBuilder.AppendLine($"ON CREATE SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp(), {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                else
                    cypherBuilder.AppendLine($"ON CREATE SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                cypherBuilder.AppendLine($"ON MATCH SET {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
            }
            else
            {
                cypherBuilder.Append($"SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                if (timestampConfig.EnforceModifiedTimestampKey)
                    cypherBuilder.Append($", {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                cypherBuilder.AppendLine();
            }
        }
        else
        {
            cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format()}({targetNodeAlias})");

        }
        cypherBuilder.AppendLine(")");
    }

    private void AppendGroupRelationCypher(StringBuilder cypherBuilder, KeyValuePair<string, Dictionary<string, Node>> relation, string variable, string alias, int nodeSetIndex, string relationAction = "MERGE")
    {
        var relationIndex = GroupRelations.Keys.ToList().IndexOf(relation.Key);
        cypherBuilder.AppendLine($"FOREACH (ignored IN CASE WHEN {variable}.{relation.Key} IS NOT NULL THEN [1] ELSE [] END |");
        foreach (var member in relation.Value)
        {
            var nextDepthVariable = ComputeAlias("muv", nodeSetIndex, relationIndex, depth + 1);
            cypherBuilder.AppendLine($"FOREACH ({nextDepthVariable} IN {variable}.{relation.Key}.{member.Key} |");
            var targetNodeAlias = member.Value.MergeRelations(cypherBuilder, nextDepthVariable, nodeSetIndex, relationIndex);
            var relationConfig = NodeConfig.Relations[relation.Key];
            var timestampConfig = Neo4jSingletonContext.TimestampConfiguration;
            if (timestampConfig.Enabled)
            {
                var relationAlias = ComputeAlias("r", nodeSetIndex, relationIndex);
                cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format(relationAlias)}({targetNodeAlias})");
                if (relationAction == "MERGE")
                {
                    if (timestampConfig.EnforceModifiedTimestampKey)
                        cypherBuilder.AppendLine($"ON CREATE SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp(), {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                    else
                        cypherBuilder.AppendLine($"ON CREATE SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                    cypherBuilder.AppendLine($"ON MATCH SET {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                }
                else
                {
                    cypherBuilder.Append($"SET {relationAlias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                    if (timestampConfig.EnforceModifiedTimestampKey)
                        cypherBuilder.Append($", {relationAlias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                    cypherBuilder.AppendLine();
                }
            }
            else
            {
                cypherBuilder.AppendLine($"{relationAction} ({alias}){relationConfig.Format()}({targetNodeAlias})");
            }
            cypherBuilder.AppendLine(")");
        }
        cypherBuilder.AppendLine(")");
    }

    private void CreateProperties(string alias, string variable, StringBuilder cypherBuilder)
    {
        cypherBuilder.Append($"CREATE ({alias}:{label})");
        var properties = Identifiers.Concat(Properties);
        AppendWithSetProperties(cypherBuilder, alias, variable, properties, false);
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
        AppendWithSetProperties(cypherBuilder, alias, variable, Properties, true);
    }
    private static void AppendWithSetProperties(StringBuilder cypherBuilder, string alias, string variable, IEnumerable<string> properties, bool isMerge)
    {
        var timestampConfig = Neo4jSingletonContext.TimestampConfiguration;
        if (isMerge && timestampConfig.Enabled)
        {
            cypherBuilder.AppendLine();
            cypherBuilder.Append($"ON CREATE SET {alias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
            if (timestampConfig.EnforceModifiedTimestampKey)
            {
                cypherBuilder.Append($", {alias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
            }
            cypherBuilder.AppendLine();
            cypherBuilder.AppendLine($"ON MATCH SET {alias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
        }
        if (properties.Any())
        {
            if (isMerge && timestampConfig.Enabled) cypherBuilder.Append("SET ");
            else cypherBuilder.Append(" SET ");
            cypherBuilder.Append(string.Join(", ", properties.Select(x => $"{alias}.{x}={variable}.{x}")));
            if (timestampConfig.Enabled && !isMerge)
            {
                cypherBuilder.Append($", {alias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
                if (timestampConfig.EnforceModifiedTimestampKey)
                {
                    cypherBuilder.Append($", {alias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
                }
            }
            cypherBuilder.AppendLine();
        }
        else if (timestampConfig.Enabled && !isMerge)
        {
            cypherBuilder.Append(" SET ");
            cypherBuilder.Append($"{alias}.{timestampConfig.CreatedTimestampKey}=timestamp()");
            if (timestampConfig.EnforceModifiedTimestampKey)
            {
                cypherBuilder.Append($", {alias}.{timestampConfig.ModifiedTimestampKey}=timestamp()");
            }
            cypherBuilder.AppendLine();
        }
        else if(!timestampConfig.Enabled)
        {
            cypherBuilder.AppendLine();
        }
    }

    private string ComputeAlias(string prefix, int nodeSetIndex, int index, int? _depth = null)
    {
        if ((_depth ?? depth) == 0) return $"{prefix}_{nodeSetIndex}";
        return $"{prefix}_{nodeSetIndex}_{_depth ?? depth}_{index}";
    }
}