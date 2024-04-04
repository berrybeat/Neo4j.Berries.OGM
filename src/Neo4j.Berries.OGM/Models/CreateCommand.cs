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

internal class CreateCommand<TNode> : ICommand
{
    private readonly int _nodeSetIndex;
    private readonly TNode _source;
    private readonly int _index;

    private StringBuilder CypherBuilder { get; }
    private int CypherLines { get; set; }

    private string Label => _source.GetType().Name;
    private string Alias => $"{Label.ToLower()}{_index}";
    private PropertyInfo[] Properties => _source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
    private NodeConfiguration NodeConfig { get; } = new NodeConfiguration();
    public Dictionary<string, object> Parameters { get; set; } = [];
    public string CurrentParameterName => $"$cp_{_nodeSetIndex}_{_index}_{Parameters.Count}";
    public string ParameterFormat => $"$cp_{_nodeSetIndex}_{_index}_{{0}}";
    public CreateCommand(TNode source, int index, int nodeSetIndex, StringBuilder cypherBuilder)
    {
        _nodeSetIndex = nodeSetIndex;
        _source = source;
        _index = index;
        CypherBuilder = cypherBuilder;
        if (Neo4jSingletonContext.Configs.TryGetValue(Label, out NodeConfiguration value))
        {
            NodeConfig = value;
        }
        AddCreateNodeCypher();
        AddSingleRelationsCyphers();
        AddRelationCollectionCypher();
    }

    private void AddCreateNodeCypher()
    {
        new PropertiesHelper(Properties, NodeConfig, _source)
            .AddNormalizedParameters(Parameters, ParameterFormat, out var safeKeyValueParameters);
        var safeParameters = BuildSafeParameters(safeKeyValueParameters);
        
        CypherBuilder.AppendLine($"CREATE ({Alias}:{Label} {{ {string.Join(", ", safeParameters)} }})");
        CypherLines++;
    }

    private void AddSingleRelationsCyphers()
    {
        var singleRelationProperties = Properties
            .Where(p => NodeConfig.Relations.ContainsKey(p.Name))
            .Where(p => !p.PropertyType.IsAssignableTo(typeof(ICollection)))
            .Where(p => p.GetValue(_source) != null);
        foreach (var prop in singleRelationProperties)
        {
            var value = prop.GetValue(_source);
            var targetNodeConfig = new NodeConfiguration();
            if (Neo4jSingletonContext.Configs.TryGetValue(prop.PropertyType.Name, out NodeConfiguration _targetNodeConfig))
            {
                targetNodeConfig = _targetNodeConfig;
            }
            var relation = NodeConfig.Relations[prop.Name];
            var targetNodeAlias = $"{relation.EndNodeType.Name.ToLower()}{_index}_{CypherLines}";
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
    private void AddRelationCollectionCypher()
    {
        var relationCollectionProperties = Properties
            .Where(p => NodeConfig.Relations.ContainsKey(p.Name))
            .Where(p => p.PropertyType.IsAssignableTo(typeof(ICollection)))
            .Where(p => p.GetValue(_source) != null)
            .Where(p => (p.GetValue(_source) as ICollection).Count > 0);
        foreach (var prop in relationCollectionProperties)
        {
            var collection = prop.GetValue(_source) as ICollection;
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
                var targetNodeAlias = $"{relation.EndNodeType.Name.ToLower()}{_index}_{CypherLines}";
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