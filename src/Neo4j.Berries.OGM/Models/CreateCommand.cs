using System.Collections;
using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Utils;

namespace Neo4j.Berries.OGM.Models;


//Important: CreateCommand is unique per NodeSet
internal class CreateCommand
{
    #region Constructor parameters
    private int NodeSetIndex { get; }
    private string UnwindVariable { get; }
    private NodeConfiguration NodeConfig { get; }
    private StringBuilder CypherBuilder { get; }
    #endregion
    public List<string> Properties { get; set; } = [];
    public Dictionary<string, List<string>> SingleRelations { get; private set; } = [];
    public Dictionary<string, List<string>> MultipleRelations { get; private set; } = [];
    protected CreateCommand() { }
    public CreateCommand(int nodeSetIndex, string unwindVariable, NodeConfiguration nodeConfig, StringBuilder cypherBuilder)
    {
        NodeSetIndex = nodeSetIndex;
        UnwindVariable = unwindVariable;
        NodeConfig = nodeConfig;
        CypherBuilder = cypherBuilder;
    }
    public void Add<TNode>(TNode node)
    {
        Dictionary<string, object> obj = node.ToDictionary(Neo4jSingletonContext.Configs);
        Add(obj);
    }
    public void Add(Dictionary<string, object> node)
    {
        var props = node.Where(x => !NodeConfig.Relations.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        Properties.AddRange(props.Keys);
        var relations = node.Where(x => NodeConfig.Relations.ContainsKey(x.Key)).Where(x => x.Value != null);
        var singleRelations = relations
            .Where(
                x => x.Value?.GetType().IsAssignableTo(typeof(IDictionary)) == true
            )
            .ToDictionary(
                x => x.Key, x => (x.Value as Dictionary<string, object>).Where(y => y.Value != null).Select(y => y.Key).ToList()
            );
        var multipleRelations = relations
            .Where(
                x => x.Value?.GetType().IsAssignableTo(typeof(IDictionary)) == false
            )
            .ToDictionary
            (
                x => x.Key,
                x => (
                    x.Value as IEnumerable<Dictionary<string, object>>
                ).SelectMany(
                    y => y
                ).Where(y => y.Value != null)
                .Select(x => x.Key)
                .ToList()
            );
        foreach (var item in singleRelations)
        {
            if (SingleRelations.TryGetValue(item.Key, out List<string> value))
            {
                value.AddRange(item.Value);
            }
            else
            {
                SingleRelations.Add(item.Key, item.Value);
            }
        }
        foreach (var item in multipleRelations)
        {
            if (MultipleRelations.TryGetValue(item.Key, out List<string> value))
            {
                value.AddRange(item.Value);
            }
            else
            {
                MultipleRelations.Add(item.Key, item.Value);
            }
        }
    }
    public void GenerateCypher(string label)
    {
        Properties = Properties.Distinct().ToList();
        var rootNodeAlias = $"node_{NodeSetIndex}";
        CypherBuilder.AppendLine($"CREATE ({rootNodeAlias}:{label} {{ {string.Join(", ", Properties.Select(x => $"{x}: {UnwindVariable}.{x}"))} }})");
        foreach (var (key, value) in SingleRelations)
        {
            CypherBuilder.AppendLine("WITH *");
            CypherBuilder.AppendLine($"WHERE {UnwindVariable}.{key} IS NOT NULL");
            var relation = NodeConfig.Relations[key];
            var groups = value.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            //Any property with the following iteration is not a nullable property
            //Any property lower than the following iteration is a nullable property
            var maxIteration = groups.Max(x => x.Value);
            var notNullableProperties = groups.Where(x => x.Value == maxIteration).Select(x => x.Key);
            var nullableProperties = groups.Where(x => x.Value < maxIteration).Select(x => x.Key);
            var nodeAlias = $"{JsonNamingPolicy.CamelCase.ConvertName(key)}_{NodeSetIndex}";
            var endNodeLabel = relation.EndNodeType != null ? relation.EndNodeType.Name : relation.EndNodeLabel;
            CypherBuilder.AppendLine(
                $"MERGE ({nodeAlias}:{endNodeLabel} {{ {string.Join(", ", notNullableProperties.Select(x => $"{x}: {UnwindVariable}.{key}.{x}"))} }})"
            );
            if (nullableProperties.Any())
                CypherBuilder.AppendLine(
                    $"SET {string.Join(", ", nullableProperties.Select(x => $"{nodeAlias}.{x} = {UnwindVariable}.{key}.{x}"))}"
            );
            CypherBuilder.AppendLine($"CREATE ({rootNodeAlias}){relation.Format()}({nodeAlias})");
        }
        foreach (var (key, value) in MultipleRelations)
        {
            var unwindVariable = $"uw_{JsonNamingPolicy.CamelCase.ConvertName(key)}_{NodeSetIndex}";
            CypherBuilder.AppendLine("WITH *");
            CypherBuilder.AppendLine($"WHERE {UnwindVariable}.{key} IS NOT NULL");
            CypherBuilder.AppendLine($"UNWIND {UnwindVariable}.{key} AS {unwindVariable}");
            var relation = NodeConfig.Relations[key];
            var groups = value.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            //Any property with the following iteration is not a nullable property
            //Any property lower than the following iteration is a nullable property
            var maxIteration = groups.Max(x => x.Value);
            var notNullableProperties = groups.Where(x => x.Value == maxIteration).Select(x => x.Key);
            var nullableProperties = groups.Where(x => x.Value < maxIteration).Select(x => x.Key);
            var nodeAlias = $"{JsonNamingPolicy.CamelCase.ConvertName(key)}_{NodeSetIndex}";
            var endNodeLabel = relation.EndNodeType != null ? relation.EndNodeType.Name : relation.EndNodeLabel;
            CypherBuilder.AppendLine(
                $"MERGE ({nodeAlias}:{endNodeLabel} {{ {string.Join(", ", notNullableProperties.Select(x => $"{x}: {unwindVariable}.{x}"))} }})"
            );
            if (nullableProperties.Any())
                CypherBuilder.AppendLine(
                    $"SET {string.Join(", ", nullableProperties.Select(x => $"{nodeAlias}.{x} = {unwindVariable}.{x}"))}"
                );
            CypherBuilder.AppendLine($"CREATE ({rootNodeAlias}){relation.Format()}({nodeAlias})");
        }
    }
}