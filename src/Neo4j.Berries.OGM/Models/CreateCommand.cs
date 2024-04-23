using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using Neo4j.Berries.OGM.Contexts;
using Neo4j.Berries.OGM.Enums;
using Neo4j.Berries.OGM.Helpers;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;

namespace Neo4j.Berries.OGM.Models;


internal class CreateCommand : ICommand
{
    #region Constructor parameters
    protected object Node { get; set; }
    protected string Label { get; set; }
    protected NodeConfiguration NodeConfig { get; set; }
    protected int ItemIndex { get; set; }
    protected int NodeSetIndex { get; set; }
    protected StringBuilder CypherBuilder { get; set; }
    protected bool Anonymous { get; set; }
    #endregion

    private int CypherLines { get; set; }
    private Dictionary<string, object> Properties => Node is Dictionary<string, object> ? Node as Dictionary<string, object> : Node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p.GetValue(Node));
    private string Alias => Anonymous ? $"a_{NodeSetIndex}_{ItemIndex}" : $"{Label.ToLower()}{ItemIndex}";
    public Dictionary<string, object> Parameters { get; set; } = [];
    public string CurrentParameterName => $"$cp_{NodeSetIndex}_{ItemIndex}_{Parameters.Count}";
    public string ParameterFormat => $"$cp_{NodeSetIndex}_{ItemIndex}_{{0}}";
    protected CreateCommand() { }
    public CreateCommand(object node, string label, NodeConfiguration nodeConfig, int itemIndex, int nodeSetIndex, StringBuilder cypherBuilder, bool anonymous)
    {
        Node = node;
        Label = label;
        NodeConfig = nodeConfig;
        ItemIndex = itemIndex;
        NodeSetIndex = nodeSetIndex;
        CypherBuilder = cypherBuilder;
        Anonymous = anonymous;
        AddCreateNodeCypher();
        AddSingleRelationsCyphers();
        AddRelationCollectionCypher();
    }

    protected void AddCreateNodeCypher()
    {
        var propertiesHelper = new PropertiesHelper(Node);
        var validProperties = propertiesHelper.GetValidProperties(NodeConfig);
        PropertiesHelper
            .AddNormalizedParameters(
                validProperties,
                Parameters,
                ParameterFormat,
                out var safeKeyValueParameters);
        var safeParameters = BuildSafeParameters(safeKeyValueParameters);

        CypherBuilder.AppendLine($"CREATE ({Alias}:{Label} {{ {string.Join(", ", safeParameters)} }})");
        CypherLines++;
    }

    protected void AddSingleRelationsCyphers()
    {
        var singleRelationProperties = Properties
            .Where(p => p.Value != null)
            .Where(p => NodeConfig.Relations.ContainsKey(p.Key))
            .Where(p => 
                (!Anonymous && !p.Value.GetType().IsAssignableTo(typeof(ICollection)) &&
                !p.Value.GetType().IsGenericType) ||
                Anonymous && p.Value.GetType().IsAssignableTo(typeof(IDictionary)));
        foreach (var prop in singleRelationProperties)
        {
            var targetNodeConfig = new NodeConfiguration();
            if (Neo4jSingletonContext.Configs.TryGetValue(prop.Key, out NodeConfiguration _targetNodeConfig))
            {
                targetNodeConfig = _targetNodeConfig;
            }
            var relation = NodeConfig.Relations[prop.Key];
            MergeRelation(prop.Value, targetNodeConfig, relation);
        }
    }
    protected void AddRelationCollectionCypher()
    {
        var collectionRelationProperties = Properties
            .Where(p => p.Value != null)
            .Where(p => NodeConfig.Relations.ContainsKey(p.Key))
            .Where(p => p.Value.GetType().IsAssignableTo(typeof(ICollection)))
            .Where(p => !p.Value.GetType().IsAssignableTo(typeof(IDictionary)))
            .Where(p => (p.Value as ICollection).Count > 0);
        foreach (var prop in collectionRelationProperties)
        {
            var collection = prop.Value as ICollection;
            var firstItem = collection.OfType<object>().First();
            var targetNodeConfig = new NodeConfiguration();
            if (Neo4jSingletonContext.Configs.TryGetValue(prop.Key, out NodeConfiguration _targetNodeConfig))
            {
                targetNodeConfig = _targetNodeConfig;
            }
            var relation = NodeConfig.Relations[prop.Key];
            foreach (var item in collection)
            {
                MergeRelation(item, targetNodeConfig, relation);
            }
        }
    }
    private void MergeRelation(object source, NodeConfiguration nodeConfig, IRelationConfiguration relation)
    {
        var propertiesHelper = new PropertiesHelper(source);
        var validProperties = propertiesHelper.GetValidProperties(nodeConfig, relation);
        PropertiesHelper.AddNormalizedParameters(validProperties, Parameters, ParameterFormat, out var safeKeyValueParameters);
        var endNodeLabel = string.IsNullOrEmpty(relation.EndNodeLabel) ? relation.EndNodeType.Name : relation.EndNodeLabel;
        var endNodeAlias = $"{endNodeLabel.ToLower()}{ItemIndex}_{CypherLines}";
        var safeParameters = BuildSafeParameters(safeKeyValueParameters);
        CypherBuilder.AppendLine($"MERGE ({endNodeAlias}:{endNodeLabel} {{ {string.Join(", ", safeParameters)} }})");
        if (relation.Direction == RelationDirection.In)
        {
            CypherBuilder.AppendLine($"CREATE ({Alias})<-[:{relation.Label}]-({endNodeAlias})");
        }
        else
        {
            CypherBuilder.AppendLine($"CREATE ({Alias})-[:{relation.Label}]->({endNodeAlias})");
        }
        CypherLines += 2;
    }
    private static IEnumerable<string> BuildSafeParameters(Dictionary<string, string> safeKeyValueParameters)
    {
        return safeKeyValueParameters.Select(x => $"{x.Key}: {x.Value}");
    }
}

internal class CreateCommand<TNode> : CreateCommand, ICommand
{
    public CreateCommand(TNode source, int itemIndex, int nodeSetIndex, StringBuilder cypherBuilder)
    {
        Node = source;
        Label = typeof(TNode).Name;
        NodeConfig = new NodeConfiguration();
        if (Neo4jSingletonContext.Configs.TryGetValue(typeof(TNode).Name, out NodeConfiguration _nodeConfig))
        {
            NodeConfig = _nodeConfig;
        }
        ItemIndex = itemIndex;
        NodeSetIndex = nodeSetIndex;
        CypherBuilder = cypherBuilder;
        Anonymous = false;
        AddCreateNodeCypher();
        AddSingleRelationsCyphers();
        AddRelationCollectionCypher();
    }
}