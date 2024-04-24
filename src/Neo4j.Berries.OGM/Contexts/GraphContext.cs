using System.Reflection;
using System.Text;
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
            var instance = Activator.CreateInstance(nodeSetProp.PropertyType, i, nodeConfig, Database, CypherBuilder);
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
        List<KeyValuePair<string, object>> parameters = [];
        GetCreateParameters(parameters);

        await Database.RunAsync(CypherBuilder.ToString(), parameters, cancellationToken);

        //This will prevent the SaveChangesAsync to save multiple times accidentally.
        ResetCreateCommands();
        CypherBuilder.Clear();
    }

    public void SaveChanges()
    {
        List<KeyValuePair<string, object>> parameters = [];
        GetCreateParameters(parameters);

        Database.Run(CypherBuilder.ToString(), parameters);

        //This will prevent the SaveChangesAsync to save multiple times accidentally.
        ResetCreateCommands();
        CypherBuilder.Clear();
    }

    private void GetCreateParameters(List<KeyValuePair<string, object>> parameters)
    {
        foreach (var nodeSet in NodeSets)
        {
            var nodeSetParams = nodeSet.CreateCommands.SelectMany(createCommand => createCommand.Parameters);
            parameters.AddRange(nodeSetParams);
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
