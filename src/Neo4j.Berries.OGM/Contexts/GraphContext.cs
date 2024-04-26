using System.Reflection;
using System.Text;
using System.Text.Json;
using Neo4j.Berries.OGM.Interfaces;
using Neo4j.Berries.OGM.Models.Config;
using Neo4j.Berries.OGM.Models.Sets;

namespace Neo4j.Berries.OGM.Contexts;

public abstract class GraphContext
{
    public DatabaseContext Database { get; private set; }
    internal StringBuilder CypherBuilder { get; } = new StringBuilder();
    internal IEnumerable<INodeSet> NodeSets { get; set; } = [];
    public GraphContext(Neo4jOptions options)
    {
        Database = new DatabaseContext(options);
        InitNodeSets();
    }

    private void InitNodeSets()
    {
        var nodeSetProps = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x =>
                x.PropertyType.IsAssignableTo(typeof(INodeSet)));
        for (var i = 0; i < nodeSetProps.Count(); i++)
        {
            var nodeSetProp = nodeSetProps.ElementAt(i);
            var nodeSetType = nodeSetProp.PropertyType.GetGenericArguments().First().Name;
            var nodeConfig = new NodeConfiguration();
            if (Neo4jSingletonContext.Configs.TryGetValue(nodeSetType, out var _nodeConfig))
            {
                nodeConfig = _nodeConfig;
            }
            //Creating the NodeSet instance
            var instance = Activator.CreateInstance(
                nodeSetProp.PropertyType, //type of the nodeset
                i, //nodeIndex
                JsonNamingPolicy.CamelCase.ConvertName(nodeSetProp.Name), //name
                nodeConfig, //nodeConfig will be passed in advance, to avoid searching for it everytime.
                Database, //This is the database context and is needed for Match related methods.
                CypherBuilder //The shared string builder, this is only used for creation.
            );
            nodeSetProp.SetValue(this, instance);
            NodeSets = NodeSets.Append(instance as INodeSet);
        }
    }
    /// <summary>
    /// Creates a new NodeSet with the given label
    /// </summary>
    /// <param name="label">The label of the anonymous node</param>
    /// <returns>The created NodeSet</returns>
    /// <remarks>IMPORTANT: The anonymous method makes the code vulnerable against cypher injection.</remarks>
    public NodeSet Anonymous(string label)
    {
        var nodeSet = new NodeSet(label, new NodeConfiguration(), NodeSets.Count(), Database, CypherBuilder);
        NodeSets = NodeSets.Append(nodeSet);
        return nodeSet;
    }
    /// <summary>
    /// Creates a new NodeSet with the given label and configuration
    /// </summary>
    /// <param name="label">The label of the anonymous node</param>
    /// <param name="builder">The configuration builder for the anonymous node</param>
    /// <returns>The created NodeSet</returns>
    /// <remarks>IMPORTANT: The anonymous method makes the code vulnerable against cypher injection.</remarks>
    public NodeSet Anonymous(string label, Action<NodeConfigurationBuilder> builder)
    {
        var configBuilder = new NodeConfigurationBuilder();
        builder(configBuilder);
        var nodeSet = new NodeSet(label, configBuilder.NodeConfiguration, NodeSets.Count(), Database, CypherBuilder);
        NodeSets = NodeSets.Append(nodeSet);
        return nodeSet;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> parameters = [];
        GetCreateParameters(parameters);
        var validNodeSets = NodeSets.Where(x => x.Nodes.Any());
        for (var i = 0; i < validNodeSets.Count(); i++)
        {
            if (i > 0 && i < validNodeSets.Count() - 1)
            {
                CypherBuilder.AppendLine("WITH *");
            }
            validNodeSets.ElementAt(i).BuildCypher();
        }
        var _parameters = parameters.ToList();
        await Database.RunAsync(CypherBuilder.ToString(), _parameters, cancellationToken);

        //This will prevent the SaveChangesAsync to save multiple times accidentally.
        ResetCreateCommands();
        CypherBuilder.Clear();
    }

    public void SaveChanges()
    {
        Dictionary<string, object> parameters = [];
        GetCreateParameters(parameters);
        var validNodeSets = NodeSets.Where(x => x.Nodes.Any());
        for (var i = 0; i < validNodeSets.Count(); i++)
        {
            if (i > 0 && i < validNodeSets.Count() - 1)
            {
                CypherBuilder.AppendLine("WITH *");
            }
            validNodeSets.ElementAt(i).BuildCypher();
        }
        Database.Run(CypherBuilder.ToString(), parameters);

        //This will prevent the SaveChangesAsync to save multiple times accidentally.
        ResetCreateCommands();
        CypherBuilder.Clear();
    }

    private void GetCreateParameters(Dictionary<string, object> parameters)
    {
        foreach (var nodeSet in NodeSets.Where(x => x.Nodes.Any()))
        {
            parameters[nodeSet.Key] = nodeSet.Nodes;
        }
    }
    private void ResetCreateCommands()
    {
        foreach (var nodeSet in NodeSets)
        {
            nodeSet.Reset();
        }
    }
}
