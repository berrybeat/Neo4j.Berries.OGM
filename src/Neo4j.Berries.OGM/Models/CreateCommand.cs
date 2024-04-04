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
    private string Alias => Anonymous ? $"a_{NodeSetIndex}_{ItemIndex}" : $"{Label.ToLower()}{ItemIndex}";
    private PropertyInfo[] Properties => Node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
        new PropertiesHelper(Properties, NodeConfig, Node)
            .AddNormalizedParameters(Parameters, ParameterFormat, out var safeKeyValueParameters);
        var safeParameters = BuildSafeParameters(safeKeyValueParameters);

        CypherBuilder.AppendLine($"CREATE ({Alias}:{Label} {{ {string.Join(", ", safeParameters)} }})");
        CypherLines++;
    }

    protected void AddSingleRelationsCyphers()
    {
        var singleRelationProperties = Properties
            .Where(p => NodeConfig.Relations.ContainsKey(p.Name))
            .Where(p => !p.PropertyType.IsAssignableTo(typeof(ICollection)))
            .Where(p => p.GetValue(Node) != null);
        foreach (var prop in singleRelationProperties)
        {
            var value = prop.GetValue(Node);
            var targetNodeConfig = new NodeConfiguration();
            if (Neo4jSingletonContext.Configs.TryGetValue(prop.PropertyType.Name, out NodeConfiguration _targetNodeConfig))
            {
                targetNodeConfig = _targetNodeConfig;
            }
            var relation = NodeConfig.Relations[prop.Name];
            var targetNodeAlias = $"{relation.EndNodeType.Name.ToLower()}{ItemIndex}_{CypherLines}";
            var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                        (relation.EndNodeMergeProperties.Any() && relation.EndNodeMergeProperties.Contains(p.Name)) ||
                        (!relation.EndNodeMergeProperties.Any() &&
                            ((!targetNodeConfig.ExcludedProperties.Contains(p.Name) && !targetNodeConfig.ExcludedProperties.IsEmpty) ||
                            (targetNodeConfig.IncludedProperties.Contains(p.Name) && !targetNodeConfig.IncludedProperties.IsEmpty) ||
                            (targetNodeConfig.ExcludedProperties.IsEmpty && targetNodeConfig.IncludedProperties.IsEmpty))
                        ))
                .Where(p => p.GetValue(value) != null);

            new PropertiesHelper(Properties, NodeConfig, value)
                .AddNormalizedParameters(properties, Parameters, ParameterFormat, out var safeKeyValueParameters);
            var safeParameters = BuildSafeParameters(safeKeyValueParameters);

            CypherBuilder.AppendLine($"MERGE ({targetNodeAlias}:{relation.EndNodeType.Name} {{ {string.Join(", ", safeParameters)} }})");
            if (relation.Direction == RelationDirection.In)
            {
                CypherBuilder.AppendLine($"CREATE ({Alias})<-[:{relation.Label}]-({targetNodeAlias})");
            }
            else
            {
                CypherBuilder.AppendLine($"CREATE ({Alias})-[:{relation.Label}]->({targetNodeAlias})");
            }
            CypherLines += 2;
        }
    }
    protected void AddRelationCollectionCypher()
    {
        var relationCollectionProperties = Properties
            .Where(p => NodeConfig.Relations.ContainsKey(p.Name))
            .Where(p => p.PropertyType.IsAssignableTo(typeof(ICollection)))
            .Where(p => p.GetValue(Node) != null)
            .Where(p => (p.GetValue(Node) as ICollection).Count > 0);
        foreach (var prop in relationCollectionProperties)
        {
            var collection = prop.GetValue(Node) as ICollection;
            var firstItem = collection.OfType<object>().First();
            var targetNodeConfig = new NodeConfiguration();
            if (Neo4jSingletonContext.Configs.TryGetValue(prop.PropertyType.Name, out NodeConfiguration _targetNodeConfig))
            {
                targetNodeConfig = _targetNodeConfig;
            }
            var relation = NodeConfig.Relations[prop.Name];
            foreach (var item in collection)
            {
                var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p =>
                        (relation.EndNodeMergeProperties.Any() && relation.EndNodeMergeProperties.Contains(p.Name)) ||
                        (!relation.EndNodeMergeProperties.Any() &&
                            ((!targetNodeConfig.ExcludedProperties.Contains(p.Name) && !targetNodeConfig.ExcludedProperties.IsEmpty) ||
                            (targetNodeConfig.IncludedProperties.Contains(p.Name) && !targetNodeConfig.IncludedProperties.IsEmpty) ||
                            (targetNodeConfig.ExcludedProperties.IsEmpty && targetNodeConfig.IncludedProperties.IsEmpty))
                        ))
                    .Where(p => p.GetValue(item) != null);
                var targetNodeAlias = $"{relation.EndNodeType.Name.ToLower()}{ItemIndex}_{CypherLines}";
                new PropertiesHelper(Properties, NodeConfig, item)
                    .AddNormalizedParameters(properties, Parameters, ParameterFormat, out var safeKeyValueParameters);
                var safeParameters = BuildSafeParameters(safeKeyValueParameters);
                CypherBuilder.AppendLine($"MERGE ({targetNodeAlias}:{relation.EndNodeType.Name} {{ {string.Join(", ", safeParameters)} }})");
                if (relation.Direction == RelationDirection.In)
                {
                    CypherBuilder.AppendLine($"CREATE ({Alias})<-[:{relation.Label}]-({targetNodeAlias})");
                }
                else
                {
                    CypherBuilder.AppendLine($"CREATE ({Alias})-[:{relation.Label}]->({targetNodeAlias})");
                }
                CypherLines += 2;
            }
        }
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