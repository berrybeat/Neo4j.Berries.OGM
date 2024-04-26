using System.Collections;
using System.Data;
using System.Text;
using System.Text.Json;
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
        AppendProperties(node);

        var relations = node.Where(x => NodeConfig.Relations.ContainsKey(x.Key)).Where(x => x.Value != null);
        AppendSingleRelations(relations);
        AppendMultipleRelations(relations);
    }
    public void GenerateCypher(string label)
    {
        var rootNodeAlias = ModifyRootCypher(rootNodeLabel: label);
        ModifyRelationCypher(isMultiple: false, rootNodeAlias: rootNodeAlias);
        ModifyRelationCypher(isMultiple: true, rootNodeAlias: rootNodeAlias);
    }

    #region Relations dictionaries preparation
    private void AppendProperties(Dictionary<string, object> node)
    {
        var props = node.Where(x => !NodeConfig.Relations.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        Properties.AddRange(props.Keys);
    }
    private void AppendSingleRelations(IEnumerable<KeyValuePair<string, object>> relations)
    {
        var singleRelations = relations
            .Where(
                x => x.Value?.IsDictionary() == true
            )
            .ToDictionary(
                x => x.Key, x => (x.Value as Dictionary<string, object>)
                    .Where(y => y.Value != null)
                    .Select(y => y.Key)
            );
        AppendRelations(singleRelations, SingleRelations);
    }
    private void AppendMultipleRelations(IEnumerable<KeyValuePair<string, object>> relations)
    {
        var multipleRelations = relations
            .Where(
                x => x.Value?.IsDictionary() == false
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
            );
        AppendRelations(multipleRelations, MultipleRelations);
    }

    private static void AppendRelations(Dictionary<string, IEnumerable<string>> from, Dictionary<string, List<string>> to)
    {
        foreach (var item in from)
        {
            if (to.TryGetValue(item.Key, out List<string> value))
            {
                value.AddRange(item.Value);
            }
            else
            {
                to.Add(item.Key, item.Value.ToList());
            }
        }
    }
    #endregion

    #region Cypher builder helpers
    //This modifies the first line which is CREATE ... 
    private string ModifyRootCypher(string rootNodeLabel)
    {
        var properties = Properties.Distinct();
        var rootNodeAlias = $"node_{NodeSetIndex}";
        CypherBuilder.AppendLine($"CREATE ({rootNodeAlias}:{rootNodeLabel} {{ {string.Join(", ", properties.Select(x => $"{x}: {UnwindVariable}.{x}"))} }})");
        return rootNodeAlias;
    }
    private void ModifyRelationCypher(bool isMultiple, string rootNodeAlias)
    {
        var relations = isMultiple ? MultipleRelations : SingleRelations;
        foreach (var (key, value) in relations)
        {
            CypherBuilder.AppendLine("WITH *");
            //Here, regardless of multiple relations or single, it should check if for example person.Companies exists
            CypherBuilder.AppendLine($"WHERE {UnwindVariable}.{key} IS NOT NULL");

            var unwindVariable = isMultiple ? $"uw_{JsonNamingPolicy.CamelCase.ConvertName(key)}_{NodeSetIndex}" : UnwindVariable;
            if (isMultiple)
                CypherBuilder.AppendLine($"UNWIND {UnwindVariable}.{key} AS {unwindVariable}");

            var relation = NodeConfig.Relations[key];

            //Grouping the properties by their iteration
            var groups = value
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            //Any property with the following iteration is not a nullable property
            //Any property lower than the following iteration is a nullable property
            var maxIteration = groups.Max(x => x.Value);
            var notNullableProperties = groups.Where(x => x.Value == maxIteration).Select(x => x.Key);
            var nullableProperties = groups.Where(x => x.Value < maxIteration).Select(x => x.Key);

            var nodeAlias = $"{JsonNamingPolicy.CamelCase.ConvertName(key)}_{NodeSetIndex}";
            var endNodeLabel = relation.EndNodeType != null ? relation.EndNodeType.Name : relation.EndNodeLabel;
            var objectPropertyPath = isMultiple ? unwindVariable : $"{unwindVariable}.{key}";
            CypherBuilder.AppendLine(
                $"MERGE ({nodeAlias}:{endNodeLabel} {{ {string.Join(", ", notNullableProperties.Select(x => $"{x}: {objectPropertyPath}.{x}"))} }})"
            );
            if (nullableProperties.Any())
                CypherBuilder.AppendLine(
                    $"SET {string.Join(", ", nullableProperties.Select(x => $"{nodeAlias}.{x} = {objectPropertyPath}.{x}"))}"
                );
            CypherBuilder.AppendLine($"CREATE ({rootNodeAlias}){relation.Format()}({nodeAlias})");
        }
    }
    #endregion
}